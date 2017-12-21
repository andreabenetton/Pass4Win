namespace Pass4WinTests
{
    using Autofac;
    using NSubstitute;
    using Pass4Win;

    public static class Setup
    {
        internal static ILifetimeScope Scope { get; set; }

        public static void InitializeContainer()
        {
            var directoryProviderMock = Substitute.For<IDirectoryProvider>();


            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(directoryProviderMock).As<IDirectoryProvider>();
            builder.RegisterInstance(new ConfigHandling()).AsSelf();
            builder.RegisterType<FrmKeyManager>().AsSelf();
            builder.RegisterType<FileSystemInterface>().AsSelf();
            Scope = builder.Build().BeginLifetimeScope();
        }
    }
}
