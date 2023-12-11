Imports System.IO
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports Serilog
Imports TransCodeMD.Config
Imports TransCodeMD.Sandbox
Imports TransCodeMD.Utilities

Public Class Startup

    Private ReadOnly _configBuilder As ConfigBuilderWrap

    Private ReadOnly _host As IHost

    Private ReadOnly _options As IOptions(Of ApplicationConfig)

    Private ReadOnly _logger As ILogger(Of Startup)

    Private ReadOnly _propMgr As ILogPropertyMgr

    Public Sub New()

        ' Build the configuration, it should fail early if there are any issues.
        _configBuilder = New ConfigBuilderWrap

        ' Create the logger.
        Call StartLogger()

        ' Add the host container for the application.
        Dim hostContainer = BuildHostContainer()
        If hostContainer Is Nothing Then
            Log.Logger.Fatal("Bootstrap: The host container is null.")
            Log.CloseAndFlush()
            Throw New Exception("Bootstarp: The host container is null.")
        Else
            _host = hostContainer
        End If

        ' Add the application options from the host container.
        _options = _host.Services.GetService(Of IOptions(Of ApplicationConfig))

        ' Set the logger for the Bootstrap class. From this point on, the logger can be used in the Bootstrap class.
        _logger = _host.Services.GetService(Of ILogger(Of Startup))()

        ' Create the property manager for the logger.
        _propMgr = New LogPropertyMgr

        ' Uncomment to add a property to the logger.
        '_propMgr.Add("SomeKey", "SomeValue")


    End Sub

    Private Sub StartLogger()

        Dim LogConfig As New LoggerConfiguration
        With LogConfig
            .ReadFrom.Configuration(_configBuilder.Configuration)
            '.Enrich.FromLogContext()
            '.WriteTo.Console
            '.WriteTo.File($"{Directory.GetCurrentDirectory}/Logs/log.txt")
            '.WriteTo.SQLite($"{Directory.GetCurrentDirectory}/Logs/log.db")
        End With

        ' Start the logger for the entire application. From this point on, the logger can be used in any class.
        Log.Logger = LogConfig.CreateLogger

    End Sub

    Private Function BuildHostContainer() As IHost

        Dim hostContainer As IHost = Nothing

        'Dim result As OperationResult = OperationResult.Ok

        Log.Logger.Debug("Bootstrap: Intializing host container (InstanceId: {InstanceId})", UniqueIdProvider.GuId)

        Try
            hostContainer = Host.CreateDefaultBuilder() _
                .ConfigureAppConfiguration(Sub(context, config)
                                               config.SetBasePath($"{Directory.GetCurrentDirectory}/config/settings")
                                               config.AddJsonFile("appsettings.json", [optional]:=True, reloadOnChange:=True)
                                               config.AddJsonFile(
                                                    $"appsettings.{If(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"), "Production")}.json",
                                                    [optional]:=True, reloadOnChange:=True)
                                               config.AddEnvironmentVariables
                                               config.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly, True)
                                           End Sub) _
                .ConfigureServices(Sub(context, services)


                                       ' Sandbox
                                       services.AddTransient(Of ISandbox, Sandbox.Sandbox)

                                       ' Utilities
                                       services.AddTransient(Of ILogPropertyMgr, LogPropertyMgr)

                                       ' App Options
                                       'services.Configure(Of ApplicationConfig)(_configBuilder.Config.GetSection("AppConfig"))
                                       services.Configure(Of ApplicationConfig)(Sub(options)
                                                                                    _configBuilder.Configuration.GetSection("AppConfig").Bind(options, Sub(c) c.BindNonPublicProperties = True)
                                                                                End Sub)
                                   End Sub) _
                .UseSerilog _
                .Build

        Catch ex As Exception

            Log.Logger.Error(ex, "Bootstrap: Error intializing the host container.")
            hostContainer = Nothing

        End Try

        Return hostContainer

    End Function

    Public Async Function Start() As Task(Of IOperationResult)

        Dim result As IOperationResult = OperationResult.Ok

        Try

            Using _logger.BeginScope(_propMgr.GetProperties)

                ' Check if the application is running in sandbox mode.
                Dim sandboxed As Boolean
                Await Task.Run(Sub() sandboxed = GetSandbox())
                If sandboxed Then Exit Try

                ' Run the scrape session.
                result = Await RunApp()

            End Using

        Catch ex As Exception
            _logger.LogError(ex, "{Method}: Error in Start()", NameOf(Start))
        Finally
            Log.CloseAndFlush()
        End Try

        Return result

    End Function

    Private Function GetSandbox() As Boolean

        Dim hasSandbox As Boolean

        If _options.Value.DevOptions.Sandbox Then

            hasSandbox = True

            Log.Logger.Information("{Method}: Initializing Sandbox", NameOf(GetSandbox))

            Dim pit As ISandbox
            pit = _host.Services.GetService(Of ISandbox)

            pit.Play()

            _logger.LogInformation("Startup: Existing Sandbox")

        End If

        Return hasSandbox

    End Function

    Private Async Function RunApp() As Task(Of IOperationResult)

        _logger.LogInformation("{Method}: Running App", NameOf(RunApp))

        ' Await time-out
        'Await Task.Delay(1000)

        Dim monitor = New Monitor

        Await monitor.RunAsync()

        Return OperationResult.Ok

    End Function

End Class
