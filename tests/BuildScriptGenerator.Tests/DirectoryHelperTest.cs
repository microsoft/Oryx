// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DirectoryHelperTest
    {
        public static TheoryData<string, string> AreSameDirectoriesTrueData
        {
            get
            {
                var data = new TheoryData<string, string>
                {
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar,
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar
                    },
                    {
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar,
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar
                    },
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(AreSameDirectoriesTrueData))]
        public void AreSameDirectories_IsTrue(string dir1, string dir2)
        {
            // Arrange & Act
            var equal = DirectoryHelper.AreSameDirectories(dir1, dir2);

            // Assert
            Assert.True(equal);
        }

        public static TheoryData<string, string> AreSameDirectoriesFalseData
        {
            get
            {
                var data = new TheoryData<string, string>
                {
                    {
                        // case-sensitive
                        Path.Combine("c:", "Foo"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "Foo"),
                        Path.Combine("c:", "foo", "bar")
                    }
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(AreSameDirectoriesFalseData))]
        public void AreSameDirectories_IsFalse(string dir1, string dir2)
        {
            // Arrange & Act
            var equal = DirectoryHelper.AreSameDirectories(dir1, dir2);

            // Assert
            Assert.False(equal);
        }

        public static TheoryData<string, string> IsSubDirectoryTrueData
        {
            get
            {
                // subdir, parentdir
                var data = new TheoryData<string, string>
                {
                    {
                        Path.Combine("c:", "foo", "bar"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo", "bar", "dir1", "dir2"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine(Path.DirectorySeparatorChar.ToString(), "foo"),
                        Path.DirectorySeparatorChar.ToString()
                    },
                    {
                        Path.GetFullPath(Path.Combine("a", "b", "c", "d", "..")),
                        Path.GetFullPath(Path.Combine("a", "b"))
                    },
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(IsSubDirectoryTrueData))]
        public void IsSubDirectory_IsTrue(string subDir, string parentDir)
        {
            // Arrange & Act
            var isSubDirectory = DirectoryHelper.IsSubDirectory(subDir, parentDir);

            // Assert
            Assert.True(isSubDirectory);
        }

        public static TheoryData<string, string> IsSubDirectoryFalseData
        {
            get
            {
                // subdir, parentdir
                var data = new TheoryData<string, string>
                {
                    // Same directory is not a sub-directory
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar,
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar
                    },
                    {
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar,
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar
                    },
                    {
                        // case-sensitive
                        Path.Combine("c:", "Foo"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo", "bar")
                    },
                    {
                        Path.Combine("a", "b", "c"),
                        Path.Combine("a", "b", "cd")
                    },
                    {
                        Path.DirectorySeparatorChar.ToString(),
                        Path.Combine(Path.DirectorySeparatorChar.ToString(), "foo")
                    },
                    {
                        Path.GetFullPath(Path.Combine("a", "b", "c", "..", "..")),
                        Path.GetFullPath(Path.Combine("a", "b"))
                    },
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(IsSubDirectoryFalseData))]
        public void IsSubDirectory_IsFalse(string subDir, string parentDir)
        {
            // Arrange & Act
            var isSubDirectory = DirectoryHelper.IsSubDirectory(subDir, parentDir);

            // Assert
            Assert.False(isSubDirectory);
        }
    }
}
