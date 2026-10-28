// Packaging only: encodes the exported PNG keyframes as an animated preview.
const fs = require('node:fs');
const path = require('node:path');
const sharp = require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const project = path.resolve(__dirname, '../..');
const root = path.join(project, 'SourceArt/BananaPossessora_Transformacao_v01');
const preview = path.join(project, 'Docs/ArtPreviews/BananaPossessora_v09');
(async () => {
  const manifest = JSON.parse(fs.readFileSync(path.join(root, 'animation.json'), 'utf8').replace(/^\uFEFF/, ''));
  const pages = await Promise.all(manifest.frames.map(f => sharp(path.join(root, f.file)).ensureAlpha().raw().toBuffer()));
  await sharp(Buffer.concat(pages), { raw: { width: 256, height: 256 * pages.length, channels: 4, pageHeight: 256 } })
    .gif({ loop: 0, delay: manifest.frames.map(f => f.duration_ms), effort: 7, colours: 128, dither: 0 })
    .toFile(path.join(preview, 'transformacao.gif'));
  const html = fs.readFileSync(path.join(preview, 'preview.html'), 'utf8');
  const js = html.match(/<script>([\s\S]*?)<\/script>/)[1];
  new (require('node:vm').Script)(js);
  const metadata = await sharp(path.join(preview, 'transformacao.gif'), { animated: true }).metadata();
  if (metadata.pages !== 6) throw new Error('Expected six animation frames');
  const qa = { preview_javascript_parses: true, gif_frames: metadata.pages, gif_delay_ms: metadata.delay, gif_loop: metadata.loop, browser_visual_check: false };
  fs.writeFileSync(path.join(preview, 'preview_qa.json'), JSON.stringify(qa, null, 2));
  console.log(JSON.stringify(qa));
})().catch(e => { console.error(e); process.exitCode = 1; });
