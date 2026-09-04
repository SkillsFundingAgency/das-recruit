import replace from "@rollup/plugin-replace";
import { terser } from "rollup-plugin-terser";
import { nodeResolve } from "@rollup/plugin-node-resolve"

export default {
    input: "wwwroot/javascripts/html-editor.js",
    output: [
        {
            file: "wwwroot/javascripts/html-editor.min.js",
            format: "es",
            name: "window",
            extend: true,
            plugins: [ terser() ]
        }
    ],
    plugins: [
        replace({
            "process.env.NODE_ENV": "'production'"
        }),
        nodeResolve()
    ]
};