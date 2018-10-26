// To load me, in an M# file add:
// ----> LoadJavascriptModule("/scripts/CustomModule1", absoluteUrl: true);
define(["require", "exports"], function (require, exports) {
    Object.defineProperty(exports, "__esModule", { value: true });
    var CustomModule1 = /** @class */ (function () {
        function CustomModule1() {
        }
        Object.defineProperty(CustomModule1, "page", {
            get: function () { return window["page"]; },
            enumerable: true,
            configurable: true
        });
        CustomModule1.run = function () {
            console.log("Hello world! I am Custom-Module-1.");
            // Note: You can use << this.page >> to hook into the page lifecycle events,
            // especially in relation to Ajax redirections.
        };
        return CustomModule1;
    }());
    exports.default = CustomModule1;
});
//# sourceMappingURL=CustomModule1.js.map