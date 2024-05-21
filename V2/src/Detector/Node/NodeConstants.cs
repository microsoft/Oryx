// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector.Node
{
    internal static class NodeConstants
    {
        public const string PlatformName = "nodejs";
        public const string NpmToolName = "npm";
        public const string YarnToolName = "yarn";
        public const string PackageJsonFileName = "package.json";
        public const string PackageLockJsonFileName = "package-lock.json";
        public const string YarnLockFileName = "yarn.lock";
        public const string YarnrcYmlName = ".yarnrc.yml";
        public const string HugoTomlFileName = "config.toml";
        public const string HugoYamlFileName = "config.yaml";
        public const string HugoJsonFileName = "config.json";
        public const string LernaJsonFileName = "lerna.json";
        public const string LageConfigJSFileName = "lage.config.js";
        public const string HugoConfigFolderName = "config";
        public const string NodeModulesDirName = "node_modules";
        public const string NodeModulesToBeDeletedName = "_del_node_modules";
        public const string NodeModulesZippedFileName = "node_modules.zip";
        public const string NodeModulesTarGzFileName = "node_modules.tar.gz";
        public const string NodeModulesFileBuildProperty = "compressedNodeModulesFile";
        public const string FlutterYamlFileName = "pubspec.yaml";
        public const string FlutterFrameworkeName = "Flutter";
        public static readonly string[] IisStartupFiles = new[]
        {
            "default.htm",
            "default.html",
            "default.asp",
            "index.htm",
            "index.html",
            "iisstart.htm",
            "default.aspx",
            "index.php",
        };

        public static readonly string[] TypicalNodeDetectionFiles = new[]
        {
            "server.js",
            "app.js",
        };

        public static readonly Dictionary<string, string> DevDependencyFrameworkKeyWordToName = new Dictionary<string, string>()
        {
            { "aurelia-cli", "Aurelia" },
            { "astro", "Astro" },
            { "@11ty/eleventy", "Eleventy" },
            { "contentful", "contentful" },
            { "elm", "Elm" },
            { "ember-cli", "Ember" },
            { "@glimmer/component", "Glimmer" },
            { "hugo-cli", "Hugo" },
            { "knockout", "KnockoutJs" },
            { "next", "Next.js" },
            { "nuxt", "Nuxt.js" },
            { "polymer-cli", "Polymer" },
            { "@stencil/core", "Stencil" },
            { "svelte", "Svelte" },
            { "typescript", "Typescript" },
            { "vuepress", "VuePress" },
            { "@vue/cli-service", "Vue.js" },
            { "gatsby", "Gatsby" },
        };

        public static readonly Dictionary<string, string> DependencyFrameworkKeyWordToName = new Dictionary<string, string>()
        {
            { "@11ty/eleventy", "Eleventy" },
            { "astro", "Astro" },
            { "contentful", "contentful" },
            { "gatsby", "Gatsby" },
            { "gridsome", "Gridsome" },
            { "@ionic/angular", "Ionic Angular" },
            { "@ionic/react", "Ionic React" },
            { "jquery", "jQuery" },
            { "lit-element", "LitElement" },
            { "marko", "Marko" },
            { "express", "Express" },
            { "meteor-node-stubs", "Meteor" },
            { "mithril", "Mithril" },
            { "next", "Next.js" },
            { "react", "React" },
            { "nuxt", "Nuxt.js" },
            { "preact", "Preact" },
            { "@scullyio/init", "Scully" },
            { "three", "Three.js" },
            { "vuepress", "VuePress" },
            { "vue", "Vue.js" },
        };

        public static readonly Dictionary<string, string> WildCardDependencies = new Dictionary<string, string>()
        {
            { "@angular", "Angular" },
            { "@remix-run", "Remix" },
        };
    }
}
