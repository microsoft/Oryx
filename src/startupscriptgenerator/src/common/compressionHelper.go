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
	err = extractTar(destinationDir, tarballFile)
	if err != nil {
		panic(fmt.Sprintf("An error occurred when trying to extract tarball '%s'.", tarballFile))
	}
}

func extractTar(dst string, tarballFile string) error {
    // Detect compression format based on file extension
    if strings.HasSuffix(tarballFile, ".tar.lz4") {
        // Use lz4 command-line tool for decompression
        return untarWithCommand(dst, tarballFile, "lz4")
    } else if strings.HasSuffix(tarballFile, ".tar.zst") {
        // Use zstd command-line tool for decompression
        return untarWithCommand(dst, tarballFile, "zstd")
    } else if strings.HasSuffix(tarballFile, ".tar.gz") || strings.HasSuffix(tarballFile, ".tgz") {
        // Use native Go gzip decompression
        return untar(dst, tarballFile)
    } else {
        return fmt.Errorf("unsupported compression format for file: %s", tarballFile)
    }
}

// untarWithCommand uses the tar command with specified compression tool
func untarWithCommand(dst string, tarballFile string, compressionTool string) error {
    cmd := exec.Command("tar", "-I", compressionTool, "-xf", tarballFile, "-C", dst)
    cmd.Stdout = os.Stdout
    cmd.Stderr = os.Stderr
    
    err := cmd.Run()
    if err != nil {
        return fmt.Errorf("failed to extract %s archive: %v", compressionTool, err)
    }
    return nil
}

// Credit to https://medium.com/@skdomino/taring-untaring-files-in-go-6b07cf56bc07
// Untar takes a destination path and a reader; a tar reader loops over the tarfile
// creating the file structure at 'dst' along the way, and writing any files
func untar(dst string, tarballFile string) error {
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
