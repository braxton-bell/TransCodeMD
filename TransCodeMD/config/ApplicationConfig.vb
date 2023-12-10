Namespace Config

    Public Interface IApplicationConfig
        'Property AdminSettings As AdminSettings
        Property DevOptions As DevOptions

    End Interface
    Public Class ApplicationConfig
        Implements IApplicationConfig

        'Public Property AdminSettings As AdminSettings Implements IApplicationConfig.AdminSettings
        Public Property DevOptions As DevOptions Implements IApplicationConfig.DevOptions

    End Class

End Namespace