﻿using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_packagereferences
    {
        ITestOutputHelper output;

        public given_packagereferences(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_privateassets_all_then_packs_library()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Newtonsoft.Json' Version='1.0.0' PrivateAssets='all' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Newtonsoft.Json.dll",
            }));
        }

        [Fact]
        public void when_privateassets_all_then_packs_transitive_libraries()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Prism.Forms' Version='7.2.0.1422' PrivateAssets='all' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Prism.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Prism.Forms.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Xamarin.Forms.Core.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Xamarin.Forms.Platform.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Xamarin.Forms.Xaml.dll",
            }));
        }

        [Fact]
        public void when_privateassets_all_and_pack_false_then_does_not_pack_transitively()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Prism.Forms' Version='7.2.0.1422' PrivateAssets='all' Pack='false' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Prism.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Prism.Forms.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Xamarin.Forms.Core.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Xamarin.Forms.Platform.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Xamarin.Forms.Xaml.dll",
            }));
        }

        [Fact]
        public void when_privateassets_pack_false_then_does_not_pack()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Newtonsoft.Json' Version='1.0.0' PrivateAssets='all' Pack='false' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Newtonsoft.Json.dll",
            }));
        }

        [Fact]
        public void when_build_kind_then_does_not_pack_msbuild()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <BuildOutputKind>build</BuildOutputKind>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Build.Tasks.Core' Version='16.6.0' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.Build.Tasks.Core",
                Kind = PackageItemKind.Dependency,
            }));
        }

        [Fact]
        public void when_build_kind_and_explicit_pack_then_packs_msbuild()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <BuildOutputKind>build</BuildOutputKind>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Build.Tasks.Core' Version='16.6.0' Pack='true' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.Build.Tasks.Core",
                Kind = PackageItemKind.Dependency,
            }));
        }

    }
}