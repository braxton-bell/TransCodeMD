Imports System.IO
Imports Microsoft.Extensions.Configuration

Public Class ConfigBuilderWrap
    Private ReadOnly _configuration As IConfiguration
    Public Sub New()
        _configuration = BuildStartupConfig()
    End Sub

    Public ReadOnly Property Configuration() As IConfiguration
        Get
            Return _configuration
        End Get
    End Property


    'The appsettings.json must be copied to the build directory for this to work.
    Private Function BuildStartupConfig() As IConfiguration

        Dim configuration As IConfiguration

        Dim configBuilder As New ConfigurationBuilder

        With configBuilder
            .SetBasePath($"{Directory.GetCurrentDirectory}/config/settings")
            .AddJsonFile("appsettings.json", [optional]:=True, reloadOnChange:=True)
            .AddJsonFile(
                $"appsettings.{If(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"), "Production")}.json",
                [optional]:=True, reloadOnChange:=True)
            .AddEnvironmentVariables
            .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly, True)
            'https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-dotnet-core-app?tabs=windowscommandprompt
            '.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("ConnectionString"))
        End With

        configuration = configBuilder.Build

        Return configuration

    End Function
End Class
