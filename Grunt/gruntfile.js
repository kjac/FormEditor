module.exports = function (grunt) {
  var isPackage = grunt.cli.tasks.indexOf("package") >= 0;

  // Project configuration.
  grunt.initConfig({
    // path to source
    sourceDir: "../Source/",

    // path to umbraco stuff
    umbracoDir: "<%= sourceDir %>Umbraco/",
    umbracoPluginDir: "<%= umbracoDir %>Plugin/",

    // path to target (site)
    targetDir: isPackage ? "temp/Package/" : grunt.option("target") || "../Dist/",
    targetPluginDir: "<%= targetDir %>App_Plugins/FormEditor/",

    // project build configuration
    projectDir: "<%= sourceDir %>Solution/FormEditor/",
    projectBuildConfig: isPackage ? "release" : grunt.option("buildConfig") || "debug",

    pkg: grunt.file.readJSON("package.json"),

    // tasks
    // - less compilation
    less: {
      dist: {
        files: {
          "<%= targetPluginDir %>css/form.css": "<%= umbracoPluginDir %>css/form.less"
        }
      }
    },
    // - simple file copy (for files that need no transformation of any kind)
    copy: {
      // - static CSS and resources
      css: {
        files: [
          { expand: true, cwd: "<%= umbracoPluginDir %>css/", src: ["**", "!**/*.less"], dest: "<%= targetPluginDir %>css/", filter: "isFile" }
        ],
      },
      // - all static resources for the editor views (the actual views are handled by concat)
      editorViews: {
        files: [
          { expand: true, cwd: "<%= umbracoPluginDir %>editor/", src: ["*/**"], dest: "<%= targetPluginDir %>editor/", filter: "isFile" }
        ],
      },
      // - all configuration views 
      configViews: {
        files: [
          { expand: true, cwd: "<%= umbracoPluginDir %>config/", src: ["**"], dest: "<%= targetPluginDir %>config/", filter: "isFile" }
        ],
      },
      // - all dashboard views 
      dashboardViews: {
        files: [
          { expand: true, cwd: "<%= umbracoPluginDir %>dashboard/", src: ["**"], dest: "<%= targetPluginDir %>dashboard/", filter: "isFile" }
        ],
      },
      // - project output dll
      bin: {
        files: [
          { expand: true, cwd: "<%= projectDir %>bin/<%= projectBuildConfig %>/", src: "FormEditor*.dll", dest: "<%= targetDir %>bin/", filter: "isFile" }
        ],
      },
      // - views for frontend rendering
      frontendViews: {
        files: [
          { expand: true, cwd: "<%= umbracoDir %>Views/", src: "**/*.cshtml", dest: "<%= targetDir %>Views/", filter: "isFile" }
        ],
      },
      // - JS for frontend rendering
      frontendJS: {
        files: [
          { expand: true, cwd: "<%= umbracoDir %>JS/", src: "**/*.js", dest: "<%= targetDir %>JS/", filter: "isFile" }
        ],
      },
      // - configuration files
      config: {
        files: [
          { expand: true, cwd: "<%= umbracoDir %>Config/", src: "*.config", dest: "<%= targetDir %>Config/", filter: "isFile" }
        ],
      },
      // - localization files
      langs: {
        files: [
          { expand: true, cwd: "<%= umbracoPluginDir %>js/langs/", src: "*.js", dest: "<%= targetPluginDir %>js/langs/", filter: "isFile" }
        ],
      },
      // - package manifest file
      manifest: {
        files: [
          { expand: true, cwd: "<%= umbracoPluginDir %>", src: "package.manifest", dest: "<%= targetPluginDir %>", filter: "isFile" }
        ],
      },
      nugetPkg: isPackage ? {
        files: [
          { expand: true, cwd: "<%= targetDir %>", src: ["**/*", "!bin", "!bin/*"], dest: "temp/NuGet/content" },
          { expand: true, cwd: "<%= targetDir %>/bin", src: ["*.dll"], dest: "temp/NuGet/lib/net40" },
          { expand: true, src: ["package.nuspec"], dest: "temp/NuGet/" }
        ]
      } : {},
      nugetBin: isPackage ? {
        files: [
          { expand: true, cwd: "<%= targetDir %>/bin", src: ["*.dll"], dest: "temp/NuGetBin/lib/net40" },
          { expand: false, src: ["package.nuspec"], dest: "temp/NuGetBin/" }
        ]
      } : {},
    },
    // - concatination tasks
    concat: {
      // - create the form editor view 
      formEditorView: {
        src: ["<%= umbracoPluginDir %>editor/form.html", "<%= umbracoPluginDir %>editor/directives.*.html"],
        dest: "<%= targetPluginDir %>editor/form.html",
      },
      // - create the form data view
      formDataView: {
        src: ["<%= umbracoPluginDir %>editor/data.html", "<%= umbracoPluginDir %>editor/directives.common.html"],
        dest: "<%= targetPluginDir %>editor/data.html",
      },
      // - create the JS
      js: {
        src: [
          "<%= umbracoPluginDir %>js/controllers/*.js",
          "<%= umbracoPluginDir %>js/directives/*.js",
          "<%= umbracoPluginDir %>js/resources/*.js",
          "<%= umbracoPluginDir %>js/services/*.js"
        ],
        dest: "<%= targetPluginDir %>js/form.js",
        options: {
          sourceMap: true
        }
      }
    },
    // - watch tasks (watch everything in seperate tasks)
    watch: {
      options: {
        atBegin: true
      },
      css: {
        files: ["<%= umbracoPluginDir %>css/*.less"],
        tasks: ["less"]
      },
      editorViews: {
        files: ["<%= copy.editorViews.files[0].cwd %><%= copy.editorViews.files[0].src %>"],
        tasks: ["copy:editorViews"]
      },
      configViews: {
        files: ["<%= copy.configViews.files[0].cwd %><%= copy.configViews.files[0].src %>"],
        tasks: ["copy:configViews"]
      },
      dashboardViews: {
        files: ["<%= copy.dashboardViews.files[0].cwd %><%= copy.dashboardViews.files[0].src %>"],
        tasks: ["copy:dashboardViews"]
      },
      bin: {
        files: ["<%= copy.bin.files[0].cwd %><%= copy.bin.files[0].src %>"],
        tasks: ["copy:bin"]
      },
      frontendViews: {
        files: ["<%= copy.frontendViews.files[0].cwd %><%= copy.frontendViews.files[0].src %>"],
        tasks: ["copy:frontendViews"]
      },
      frontendJS: {
        files: ["<%= copy.frontendJS.files[0].cwd %><%= copy.frontendJS.files[0].src %>"],
        tasks: ["copy:frontendJS"]
      },
      config: {
        files: ["<%= copy.config.files[0].cwd %><%= copy.config.files[0].src %>"],
        tasks: ["copy:config"]
      },
      langs: {
        files: ["<%= copy.langs.files[0].cwd %><%= copy.langs.files[0].src %>"],
        tasks: ["copy:langs"]
      },
      manifest: {
        files: ["<%= copy.manifest.files[0].cwd %><%= copy.manifest.files[0].src %>"],
        tasks: ["copy:manifest"]
      },
      formEditorView: {
        files: ["<%= concat.formEditorView.src %>"],
        tasks: ["concat:formEditorView"]
      },
      formDataView: {
        files: ["<%= concat.formDataView.src %>"],
        tasks: ["concat:formDataView"]
      },
      js: {
        files: ["<%= concat.js.src %>"],
        tasks: ["concat:js"]
      }
    },
    msbuild: {
      options: {
        stdout: true,
        verbosity: "quiet",
        maxCpuCount: 4,
        version: 15,
        buildParameters: {
          WarningLevel: 2,
          NoWarn: 1607
        }
      },
      pkg: {
        src: ["<%= projectDir %>FormEditor.csproj"],
        options: {
          projectConfiguration: "<%= projectBuildConfig %>",
          targets: ["Clean", "Rebuild"]
        }
      }
    },
    assemblyinfo: {
      options: {
        files: ["<%= msbuild.pkg.src[0] %>"],
        info: {
          version: "<%= pkg.meta.version %>",
          fileVersion: "<%= pkg.meta.version %>"
        }
      }
    },
    clean: {
      pkg: ["temp"]
    },
    umbracoPackage: {
      pkg: {
        src: "temp/Package/",
        dest: "../Package",
        options: {
          // Options for the package.xml manifest 
          name: "Form Editor",
          version: "<%= pkg.meta.version %>",
          url: "https://github.com/kjac/FormEditor",
          license: "MIT",
          licenseUrl: "https://opensource.org/licenses/MIT",
          author: "Kenn Jacobsen",
          authorUrl: "http://our.umbraco.org/member/25455",
          readme: "See https://github.com/kjac/FormEditor for documentation."
        }
      }
    },
    template: {
      pkg: {
        options: {
          data: {
            id: "FormEditor",
            version: "<%= pkg.meta.version %>",
            title: "Form Editor"
          }
        },
        files: {
          "temp/NuGet/package.nuspec": ["package.nuspec"]
        }
      },
      bin: {
        options: {
          data: {
            id: "FormEditor.Binaries",
            version: "<%= pkg.meta.version %>",
            title: "Form Editor Binaries"
          }
        },
        files: {
          "temp/NuGetBin/package.nuspec": ["package.nuspec"]
        }
      }
    },
    nugetpack: {
      pkg: {
        src: "temp/NuGet/package.nuspec",
        dest: "../Package"
      },
      bin: {
        src: "temp/NuGetBin/package.nuspec",
        dest: "../Package"
      }
    }
  });

  // Load the plugins 
  grunt.loadNpmTasks("grunt-contrib-copy");
  grunt.loadNpmTasks("grunt-contrib-watch");
  grunt.loadNpmTasks("grunt-contrib-less");
  grunt.loadNpmTasks("grunt-contrib-concat");
  grunt.loadNpmTasks("grunt-contrib-clean");
  grunt.loadNpmTasks("grunt-msbuild");
  grunt.loadNpmTasks("grunt-umbraco-package");
  grunt.loadNpmTasks("grunt-dotnet-assembly-info");
  grunt.loadNpmTasks("grunt-nuget");
  grunt.loadNpmTasks("grunt-template");

  // Tasks
  grunt.registerTask("default", ["less", "concat", "copy"]);
  grunt.registerTask("package", ["clean", "assemblyinfo", "msbuild", "less", "concat", "copy", "umbracoPackage", "template", "nugetpack"]);
};