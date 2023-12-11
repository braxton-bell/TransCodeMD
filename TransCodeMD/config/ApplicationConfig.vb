Namespace Config

    Public Interface IApplicationConfig
        'Property AdminSettings As AdminSettings
        Property DevOptions As DevOptions

        Property LanguageMappings As Dictionary(Of String, String)
        'LanguageMappingOptions

    End Interface
    Public Class ApplicationConfig
        Implements IApplicationConfig

        'Public Property AdminSettings As AdminSettings Implements IApplicationConfig.AdminSettings
        Public Property DevOptions As DevOptions Implements IApplicationConfig.DevOptions
        Public Property LanguageMappings As Dictionary(Of String, String) Implements IApplicationConfig.LanguageMappings
        'Public Property LanguageMappings As Dictionary(Of String, String)

        'As LanguageMappingOptions Implements IApplicationConfig.LanguageMappings
    End Class

    'Public Class LanguageMappingOptions
    '    Public Property LanguageMappings As Dictionary(Of String, String)
    'End Class


End Namespace