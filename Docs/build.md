# Building and contributing
Form Editor uses [Grunt](http://gruntjs.com/) to continuously build the package during development. You'll need [Node.js](https://nodejs.org/) installed to run Grunt.

The Grunt setup for Form Editor is placed in [/Grunt/](../Grunt/). 

## Installing dependencies
Before you can run Grunt you need to install the dependencies. Bring up a terminal, `cd` to */Grunt/* and run `npm install`.

## Running Grunt
Once the dependencies are installed, simply run `grunt` in your terminal (provided your terminal is still at */Grunt/*). This will build the entire package to */Dist/*, and you can then copy the output to the root of your Umbraco site. 

To run a continuous build, run `grunt watch`. This will monitor changes made to any part of the package and rebuild the applicable parts of the package accordingly.

If you want to build to another destination folder than the default one (for example directly to your site), add the destination folder as the `target` parameter to Grunt: `grunt --target="C:\inetpub\site\"`. And of course you can mix this with the `watch` command to run a continuous build to your destination folder: `grunt watch --target="C:\inetpub\site\"`.

## Contributing
Want to contribute to Form Editor? Great! Much obliged. 

Here are a few guidelines.

### Simplicity over flexibility
Form Editor aims to provide a simple and intuitive editorial experience. You should always prioritize simplicity over flexibility when implementing features that extend into the editor UI.

A good example of this is the fields implementation. It could have been done with just a few generic field types and a library of validations (e.g. email, URL) to choose from, which would probably have been super flexible. Instead this is implemented as a bunch of specific fields types (e.g. email field, URL field), each of which implicitly is its own validation. The benefit of the chosen implementation is a simpler and more intuitive editor experience.

### Code style
Please (pretty please) observe and adapt the code style used throughout the codebase. This keeps the code easier to read and maintain and helps to prevent deterioration. 
