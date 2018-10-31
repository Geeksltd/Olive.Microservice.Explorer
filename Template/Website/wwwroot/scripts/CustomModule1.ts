
// To load me, in an M# file add:
// ----> LoadJavascriptModule("/scripts/CustomModule1", absoluteUrl: true);

// Dependencies:
import OlivePage from "olive/olivePage"

export default class CustomModule1 {

    static get page(): OlivePage { return window["page"]; }

    public static run(): void {
        console.log("Hello world! I am Custom-Module-1.");

        // Note: You can use << this.page >> to hook into the page lifecycle events,
        // especially in relation to Ajax redirections.
    }
}