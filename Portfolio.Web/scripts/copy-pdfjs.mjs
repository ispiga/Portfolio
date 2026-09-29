import { cp, copyFile, mkdir, rm } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import path from "node:path";

const packageRoot = fileURLToPath(new URL("../node_modules/pdfjs-dist/", import.meta.url));
const destination = fileURLToPath(new URL("../wwwroot/js/vendor/pdfjs/", import.meta.url));

await rm(destination, { recursive: true, force: true });
await mkdir(destination, { recursive: true });
await copyFile(path.join(packageRoot, "build/pdf.min.mjs"), path.join(destination, "pdf.min.mjs"));
await copyFile(path.join(packageRoot, "build/pdf.worker.min.mjs"), path.join(destination, "pdf.worker.min.mjs"));
await copyFile(path.join(packageRoot, "LICENSE"), path.join(destination, "LICENSE"));
await cp(path.join(packageRoot, "cmaps"), path.join(destination, "cmaps"), { recursive: true });
await cp(path.join(packageRoot, "standard_fonts"), path.join(destination, "standard_fonts"), { recursive: true });
