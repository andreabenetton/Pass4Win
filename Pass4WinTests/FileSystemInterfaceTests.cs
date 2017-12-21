namespace Pass4WinTests
{
    using System.Collections.Generic;
    using System.IO;
    using Autofac;
    using NSubstitute;
    using NUnit.Framework;
    using Pass4Win;
    using Shouldly;

    [TestFixture]
    public class FileSystemInterfaceTests
    {
        [SetUp]
        public void BeforeTest()
        {
            Setup.InitializeContainer();
        }
       
        [Test]
        public void FindMixedCaseFileWithLowerCaseSearch()
        {
            var fileNameToFind = "caseSensitiveFile.gpg";
            var anotherFile = "somefile.gpg";

            var fileMockOne = Substitute.For<IFileProvider>();
            var fileMockTwo = Substitute.For<IFileProvider>();
            
            fileMockOne.Name = fileNameToFind;
            fileMockOne.FullName = fileNameToFind;
            
            fileMockTwo.Name = anotherFile;
            fileMockTwo.FullName = anotherFile;

            var fileProviderMockList = new List<IFileProvider>
            {
                fileMockOne,
                fileMockTwo
            };

            var directoryProvider = Setup.Scope.Resolve<IDirectoryProvider>();
            directoryProvider.GetFiles(Arg.Any<string>(), Arg.Is<SearchOption>(so => so == SearchOption.AllDirectories)).Returns(fileProviderMockList.ToArray());
            
            var fsi = Setup.Scope.Resolve<FileSystemInterface>();
            fsi.Search(fileNameToFind.ToLower());
            fsi.SearchList.Count.ShouldBe(1);
            fsi.SearchList[0].ShouldBe(fileNameToFind);
        }

        [Test]
        public void FindMixedCaseFileWithMixedCaseSearch()
        {
            var fileNameToFind = "caseSensitiveFile.gpg";
            var anotherFile = "somefile.gpg";

            var fileMockOne = Substitute.For<IFileProvider>();
            var fileMockTwo = Substitute.For<IFileProvider>();

            fileMockOne.Name = fileNameToFind;
            fileMockOne.FullName = fileNameToFind;

            fileMockTwo.Name = anotherFile;
            fileMockTwo.FullName = anotherFile;
            var fileProviderMockList = new List<IFileProvider>
            {
                fileMockOne,
                fileMockTwo
            };

            var directoryProvider = Setup.Scope.Resolve<IDirectoryProvider>();
            directoryProvider.GetFiles(Arg.Any<string>(), Arg.Is<SearchOption>(so => so == SearchOption.AllDirectories)).Returns(fileProviderMockList.ToArray());

            var fsi = Setup.Scope.Resolve<FileSystemInterface>();
            fsi.Search(fileNameToFind.ToLower());
            fsi.SearchList.Count.ShouldBe(1);
            fsi.SearchList[0].ShouldBe(fileNameToFind);
        }

        [Test]
        public void FindAllFilesWithWildcardSearch()
        {
            var file = "caseSensitiveFile.gpg";
            var anotherFile = "somefile.gpg";

            var fileMockOne = Substitute.For<IFileProvider>();
            var fileMockTwo = Substitute.For<IFileProvider>();

            fileMockOne.Name = file;
            fileMockOne.FullName = anotherFile;

            fileMockTwo.Name = anotherFile;
            fileMockTwo.FullName = anotherFile;
            var fileProviderMockList = new List<IFileProvider>
            {
                fileMockOne,
                fileMockTwo
            };

            var directoryProvider = Setup.Scope.Resolve<IDirectoryProvider>();
            directoryProvider.GetFiles(Arg.Any<string>(), Arg.Is<SearchOption>(so => so == SearchOption.AllDirectories)).Returns(fileProviderMockList.ToArray());

            var fsi = Setup.Scope.Resolve<FileSystemInterface>();
            fsi.Search("*.*");
            fsi.SearchList.Count.ShouldBe(2);
            fsi.SearchList[0].ShouldBe(file);
            fsi.SearchList[1].ShouldBe(anotherFile);
        }

        /// <summary>
        ///     Tests creating of directory tree nodes
        /// </summary>
        [Test]
        public void CreateDirectoryTreeNodes()
        {
            var directoryOne = Substitute.For<IDirectoryProvider>();

            directoryOne.FullName = "directoryone";
            directoryOne.Name = "directoryone";

            directoryOne.GetDirectories().Returns(new List<IDirectoryProvider>());

            Setup.Scope.Resolve<IDirectoryProvider>()
                .GetDirectories()
                .Returns(new List<IDirectoryProvider>
                {
                    directoryOne
                });

            var fsi = Setup.Scope.Resolve<FileSystemInterface>();
            var nodes = fsi.UpdateDirectoryTree();
            nodes.ShouldNotBeNull();
            nodes.Length.ShouldBe(1);
        }

        /// <summary>
        ///     Tests creating a list files from a given directory
        /// </summary>
        [Test]
        public void GetFileList()
        {
            var file = "afile.gpg";
            var anotherFile = "somefile.gpg";

            var fileMockOne = Substitute.For<IFileProvider>();
            var fileMockTwo = Substitute.For<IFileProvider>();

            fileMockOne.Name = file;
            fileMockOne.FullName = file;
            fileMockOne.Extension = ".gpg";
            fileMockTwo.Name = anotherFile;
            fileMockTwo.FullName = anotherFile;
            fileMockTwo.Extension = ".gpg";
            var fileProviderMockList = new List<IFileProvider>
            {
                fileMockOne,
                fileMockTwo
            };

            var directoryProviderMock = Setup.Scope.Resolve<IDirectoryProvider>();
            directoryProviderMock.GetFiles().Returns(fileProviderMockList.ToArray());
            
            var fsi = Setup.Scope.Resolve<FileSystemInterface>();
            var list = fsi.UpdateDirectoryList(directoryProviderMock);
            list.Count.ShouldBe(2);
        }
    }
}