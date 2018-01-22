

Note:
To add 3rd party libraries:
    * Add the package reference in bower.json under "dependencies".
    * Then on the page that you want to load it (or in your layout cshtml file) use:

      loadLibrary('some-name', 'relative/path', ['dependency1', 'dependency2']);

Parameters:
- 'some-name' is a unique name that you assign to this library for requireJs.
- "relative/path" is the relative path of the main .JS file of the library under wwwroot/lib, and without .js extension.