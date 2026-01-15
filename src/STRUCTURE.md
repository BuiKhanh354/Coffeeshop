# Cấu trúc Folder src

```
src/
└── CoffeeShop.Web/
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── CoffeeShop.Web.csproj
    ├── package.json
    ├── Program.cs
    ├── tailwind.config.js
    │
    ├── Controllers/
    │   └── HomeController.cs
    │
    ├── Models/
    │   └── ErrorViewModel.cs
    │
    ├── Properties/
    │   └── launchSettings.json
    │
    ├── Views/
    │   ├── _ViewImports.cshtml
    │   ├── _ViewStart.cshtml
    │   ├── Home/
    │   │   ├── Index.cshtml
    │   │   └── Privacy.cshtml
    │   └── Shared/
    │       ├── _Layout.cshtml
    │       ├── _Layout.cshtml.css
    │       ├── _ValidationScriptsPartial.cshtml
    │       └── Error.cshtml
    │
    ├── wwwroot/
    │   ├── css/
    │   │   ├── app.css
    │   │   ├── output.css
    │   │   ├── site.css
    │   │   └── tailwind.css
    │   ├── js/
    │   │   ├── site.js
    │   │   └── theme.js
    │   └── lib/
    │       ├── jquery/
    │       │   └── LICENSE.txt
    │       ├── jquery-validation/
    │       │   └── LICENSE.md
    │       └── jquery-validation-unobtrusive/
    │           ├── jquery.validate.unobtrusive.js
    │           ├── jquery.validate.unobtrusive.min.js
    │           └── LICENSE.txt
    │
    ├── bin/
    │   └── Debug/
    │       └── net8.0/
    │           ├── appsettings.Development.json
    │           ├── appsettings.json
    │           ├── CoffeeShop.Web.deps.json
    │           ├── CoffeeShop.Web.runtimeconfig.json
    │           ├── CoffeeShop.Web.staticwebassets.runtime.json
    │           └── package.json
    │
    └── obj/
        ├── CoffeeShop.Web.csproj.nuget.dgspec.json
        ├── CoffeeShop.Web.csproj.nuget.g.props
        ├── CoffeeShop.Web.csproj.nuget.g.targets
        ├── project.assets.json
        └── Debug/
            └── net8.0/
                ├── CoffeeShop.Web.AssemblyInfo.cs
                ├── CoffeeShop.Web.csproj.FileListAbsolute.txt
                ├── CoffeeShop.Web.GeneratedMSBuildEditorConfig.editorconfig
                ├── CoffeeShop.Web.GlobalUsings.g.cs
                ├── CoffeeShop.Web.RazorAssemblyInfo.cs
                ├── CoffeeShop.Web.sourcelink.json
                ├── staticwebassets.build.json
                ├── staticwebassets.development.json
                ├── staticwebassets.pack.json
                ├── ref/
                ├── refint/
                ├── scopedcss/
                │   ├── bundle/
                │   │   └── CoffeeShop.Web.styles.css
                │   ├── projectbundle/
                │   │   └── CoffeeShop.Web.bundle.scp.css
                │   └── Views/
                │       └── Shared/
                │           └── _Layout.cshtml.rz.scp.css
                └── staticwebassets/
                    ├── msbuild.build.CoffeeShop.Web.props
                    ├── msbuild.buildMultiTargeting.CoffeeShop.Web.props
                    ├── msbuild.buildTransitive.CoffeeShop.Web.props
                    └── msbuild.CoffeeShop.Web.Microsoft.AspNetCore.StaticWebAssets.props
```

## Mô tả các thư mục chính

| Thư mục | Mô tả |
|---------|-------|
| **Controllers/** | Chứa các controller (HomeController.cs) |
| **Models/** | Chứa các model (ErrorViewModel.cs) |
| **Views/** | Chứa các Razor view (.cshtml) - tách thành Home và Shared |
| **Properties/** | Cấu hình khởi chạy (launchSettings.json) |
| **wwwroot/** | Thư mục tĩnh công khai (CSS, JavaScript, thư viện) |
| **bin/** | Output sau biên dịch (Debug) |
| **obj/** | Các tệp trung gian của quá trình biên dịch |

## Tệp cấu hình chính

- `Program.cs` - Điểm vào chính của ứng dụng
- `CoffeeShop.Web.csproj` - File cấu hình project
- `appsettings.json` - Cấu hình production
- `appsettings.Development.json` - Cấu hình development
- `tailwind.config.js` - Cấu hình Tailwind CSS
- `package.json` - Dependencies của Node.js
