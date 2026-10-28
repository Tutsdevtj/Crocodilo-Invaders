const sharp = require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '../..');
const out = path.join(root, 'Assets/Art/UI/MacaquiniHUD_v14');
const src = path.join(root, 'SourceArt/MacaquiniHUD_v14');
async function portrait(file, crop, name) {
  const art = await sharp(path.join(root, file)).extract(crop).resize(104,104,{kernel:'nearest'}).toBuffer();
  const disk = Buffer.from('<svg width="104" height="104"><circle cx="52" cy="52" r="51" fill="white"/></svg>');
  await sharp({create:{width:104,height:104,channels:4,background:'#092215'}})
    .composite([{input:art},{input:disk,blend:'dest-in'}]).png().toFile(path.join(out,name+'.png'));
}
(async()=>{
  fs.mkdirSync(out,{recursive:true});
  await sharp(path.join(src,'jungle_frame_source.png')).resize(500,167,{kernel:'nearest'}).png().toFile(path.join(out,'jungle_frame.png'));
  await sharp(path.join(src,'jungle_frame_phase2_source.png')).resize(500,167,{kernel:'nearest'}).png().toFile(path.join(out,'jungle_frame_half.png'));
  await portrait('Assets/Art/JungleStage/Macaquini_v13/flight_00.png',{left:183,top:196,width:176,height:176},'hud_portrait');
  await portrait('Assets/Art/JungleStage/Macaquini_v13/transform_29.png',{left:210,top:204,width:120,height:120},'hud_portrait_half');
  fs.copyFileSync(path.join(root,'Assets/Art/JungleStage/Macaquini_v05/hud_title.png'),path.join(out,'hud_title.png'));
  fs.writeFileSync(path.join(src,'provenance.json'),JSON.stringify({tool:'image_gen.imagegen',mode:'built-in',frame:'jungle_frame_source.png',phaseTwoFrame:'jungle_frame_phase2_source.png',phaseTwoPrompt:'Edit the exact frame with thorny vines, ripped banana scraps, splintered reddish copper wood and amber highlights. Preserve track and portrait opening placement and transparency; no text or characters.',prompt:'Single transparent tropical hardwood/bamboo pixel-art boss health frame, jungle leaves and vines, emerald alien veins, banana-gold accents, empty dark green health track and right portrait opening; no text or character.',portraits:'Exact crops of existing Macaquini flight_00 v13 and transform_29 v13 (angry roar); no character regeneration.',export:'Tools/Art/prepare_macaquini_hud_v14.cjs',layout:{size:[500,167],fill:[55,56,332,20],portrait:[395,40,104,104]}},null,2));
})();
