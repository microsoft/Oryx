// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"archive/tar"
	"compress/gzip"
	"fmt"
	"io"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
)

// Untar the output file to the provided destination directory
func ExtractTarball(tarballFile string, destinationDir string) {
	if tarballFile == "" {
		panic("Tarball file path is required.")
	}

	if destinationDir == "" {
		panic("Destination directory is required.")
	}

	if !PathExists(tarballFile) {
		panic(fmt.Sprintf("Could not find file '%s'.", tarballFile))
	}

	if PathExists(destinationDir) {
		println(fmt.Sprintf("Deleting existing directory '%s'...", destinationDir))
		err := os.Remove(destinationDir)
		if err != nil {
			log.Fatal(err)
		}
	}

	err := os.MkdirAll(destinationDir, 0755)
	if err != nil {
		panic(fmt.Sprintf("An error occurred when trying to create directory structure '%s'.", destinationDir))
	}

	println(fmt.Sprintf("Extracting '%s' to directory '%s'...", tarballFile, destinationDir))
	err = untar(destinationDir, tarballFile)
	if err != nil {
		panic(fmt.Sprintf("An error occurred when trying to extract tarball '%s'.", tarballFile))
	}
}

func untar(dst string, tarballFile string) error {
    // Detect compression format based on file extension
	if strings.HasSuffix(tarballFile, ".tar.zst") {
        // Use zstd for decompression
		println(fmt.Sprintf("Using zstd for decompression of file: %s", tarballFile))
        return untarWithZstd(dst, tarballFile)
    } else if strings.HasSuffix(tarballFile, ".tar.gz") {
        // Use native Go gzip decompression
		println(fmt.Sprintf("Using gzip for decompression of file: %s", tarballFile))
        return untarWithGzip(dst, tarballFile)
    } else {
        errMsg := fmt.Errorf("unsupported compression format for file: %s", tarballFile)
        Println(errMsg)
        return errMsg
	}
}

// This extracts a zstd-compressed tarball using the system tar command.
// Unlike untarWithGzip, we use the external tar binary because
// Go's standard library doesn't include zstd support, avoiding the need for third-party dependencies.
func untarWithZstd(dst string, tarballFile string) error {
    cmd := exec.Command("tar", "-I", "zstd", "-xf", tarballFile, "-C", dst)
    output, err := cmd.CombinedOutput()
    if err != nil {
        errMsg := fmt.Errorf("zstd tar extraction failed: %v, output: %s", err, output)
        Println(errMsg)
        return errMsg
    }
    println(fmt.Sprintf("zstd tar Successfully extracted '%s' to '%s'", tarballFile, dst))
    return nil
}

// Credit to https://medium.com/@skdomino/taring-untaring-files-in-go-6b07cf56bc07
// Untar takes a destination path and a reader; a tar reader loops over the tarfile
// creating the file structure at 'dst' along the way, and writing any files
func untarWithGzip(dst string, tarballFile string) error {
	r, err := os.Open(tarballFile)
	if err != nil {
		return err
	}

	gzr, err := gzip.NewReader(r)
	if err != nil {
		return err
	}
	defer gzr.Close()

	tr := tar.NewReader(gzr)

	for {
		header, err := tr.Next()

		switch {

		// if no more files are found return
		case err == io.EOF:
			return nil

		// return any other error
		case err != nil:
			return err

		// if the header is nil, just skip it (not sure how this happens)
		case header == nil:
			continue
		}

		// the target location where the dir/file should be created
		target := filepath.Join(dst, header.Name)

		// the following switch could also be done using fi.Mode(), not sure if there
		// a benefit of using one vs. the other.
		// fi := header.FileInfo()

		// check the file type
		switch header.Typeflag {

		// if its a dir and it doesn't exist create it
		case tar.TypeDir:
			if _, err := os.Stat(target); err != nil {
				if err := os.MkdirAll(target, 0755); err != nil {
					return err
				}
			}

		// if it's a file create it
		case tar.TypeReg:
			f, err := os.OpenFile(target, os.O_CREATE|os.O_RDWR, os.FileMode(header.Mode))
			if err != nil {
				return err
			}

			// copy over contents
			if _, err := io.Copy(f, tr); err != nil {
				return err
			}

			// manually close here after each file operation; defering would cause each file close
			// to wait until all operations have completed.
			f.Close()
		}
	}
}
