Namespace Config

    Public Interface IApplicationConfig
        Property AdminSettings As AdminSettings
        Property DevOptions As DevOptions
        Property LanguageMappings As Dictionary(Of String, String)

    End Interface
    Public Class ApplicationConfig
        Implements IApplicationConfig
        Public Property DevOptions As DevOptions Implements IApplicationConfig.DevOptions
        Public Property LanguageMappings As Dictionary(Of String, String) Implements IApplicationConfig.LanguageMappings
        Public Property AdminSettings As AdminSettings Implements IApplicationConfig.AdminSettings

    End Class

End Namespace