### HXIME v0.9 Chinese Input Keyboard VRChat世界用中文输入法+键盘 VRChat用中国語入力+キーボード

作者还不会用git，所以非常感谢帮忙提交PR的大家。

还在苦恼没有办法在世界里痛快地打中文吗？就用HXIME把你憋了那么多的中文通通打出来吧！
支持全拼，简拼，混拼，中英切换，简体繁体切换。安装简便，还可以自定义各种皮肤哦。

Are you still worried about not being able to type Chinese in the world? Just use HXIME to type out all the Chinese you have been holding back!
Supports full spelling, simplified spelling, mixed spelling, switching between Chinese and English, and switching between simplified and traditional Chinese. Easy to install, and you can also customize various skins.

ワールドでの中国語入力を悩んでいるあなた、HXIMEを使ってみませんか。なんと、何英文字を入力するだけで候補が出るピンインの簡単スペル機能も備えてる一方、英語との切り替えも、簡体字と繫体字の切り替え機能も備えています！メニューを開いて見ると、外観もカスタマイズできます！

“让汉字跨越次元，让文明生生不息”

“Let those characters transcend dimensions and let civilization thrive”

「漢字が次元を超え、文明が栄えるように」

Project Link: https://github.com/xianglong90II/VRChatChineseIME_HXIME

# 安装方法　Installation method　インストール：
- 把HXIME_Pinyin预制件拖进地图里。
- 你可以把SwitchBarHandle,InputBarHandle,KeyboardHandle,调整到你喜欢的位置上。
- 默认会使用第一个皮肤，你可以调整皮肤的顺序，实现更换默认皮肤。
- （注意，皮肤描述和图片必须一一对应哦。当然，觉得有些不太合适的皮肤可以移除）
- 在HXIME_Pinyin的HXIMEUI里面，目标输入框那地方选择你的目标输入框
- 完成！
------
- Drag the HXIME_Pinyin prefab into the map.
- You can adjust SwitchBarHandle, InputBarHandle, KeyboardHandle to your favorite position.
- The first skin will be used by default. You can adjust the order of the skins to change the default skin.
- (Note that the skin description and the image must correspond one to one. Of course, if you think some skins are not suitable, you can remove them)
- In the HXIMEUI of HXIME_Pinyin, select your target input box in the target input box
- Done!
------
- HXIME_Pinyin プレハブをドラッグドロップします。
- SwitchBarHandle、InputBarHandle、KeyboardHandle を好きな位置に調整できる。
- （IMEの外観のプリセットを、以下「スキン」と称する」）
- デフォルトの外観では最初のスキンが使用されます。スキンの順序を調整して、デフォルトのスキンを置き換えることができる。
- （スキンの説明と画像は1対1で対応している必要があります。もちろん、スキンが適切でないと思われる場合は削除できます）
- HXIME_PinyinのHXIMEUIで、TargetInputFieldで入力したいTargetInputFieldを選択します。
- 仕上げる！

# Q&A：
- Q: 我想要把输入法固定在世界里，不想让玩家抓取怎么办？
- A: 把SwitchBarHandle, InputBarHandle, KeyboardHandle 的VRC PickUp删掉
- Q：我不希望用户能碰到开关键盘，开关握把之类的按钮怎么办？
- A: 在SwitchBar或SettingsPanel下的Buttons里面选中你想要隐藏的按钮，把检查器左上角的勾去掉就行。
------
- Q: I want this IME imobile, what should I do if I don't want the player to grab it?
- A: Delete the VRC PickUp of SwitchBarHandle, InputBarHandle, KeyboardHandle
- Q: I don't want the user to be able to touch the switch keyboard, switch handle and other buttons, what should I do?
- A: Select the button you want to hide in Buttons under SwitchBar or SettingsPanel, and remove the check in the upper left corner of the inspector.
------
- Q: IMEを固定し、プレイヤーに取られたくない場合はどうすればいいですか?
- A: SwitchBarHandle、InputBarHandle、KeyboardHandleのVRC PickUpを削除します。
- Q: スイッチキーボードやスイッチハンドルやその他のボタンにユーザーが触れないようにしたいです。どうすればいいですか？
- A: SwitchBar または SettingsPanel の下の Buttons で非表示にしたいボタンを選択し、インスペクタの左上隅のチェックを外します。

# 进阶 Advanced 上級者向け：
- Q: 可以制作自己的皮肤吗？
- A: 当然可以！您只需要写好皮肤描述和准备一张图片作为皮肤背景。皮肤描述的格式为: 皮肤标题;皮肤描述;按钮颜色的rgba;主色1的rgba;主色2的rgba
- 例如：
- HXIME白;作者: HX2 xianglong90;(255,255,255,128);(52,161,255,255);(52,255,209,255)
- 想要精确绘制皮肤，在Themes文件夹里可以找到ThemePSDInstruction.psd文件和png文件辅助您绘制！
- 对于皮肤背景，HXIME使用了Texture2D而非UISprite。
- 也就是说如果愿意折腾的话，您甚至可以尝试线上加载或是使用RenderTexture(即用游戏内摄像机拍摄)
- Q: 可以放多少套皮肤？
- A：理论上基本无限。但是玩家目前只有前9个皮肤有槽位可以选。当然，可以从装有HXIMEUI这个脚本的物体上调用SetSkin(索引)这个公开方法来设置第10套及后面的皮肤。
- Q: 我想要导入自己的字词库怎么办？
- A: PinyinEngine里面有简体和繁体用的字词库。可以加载txt字典对默认字词库进行替换。格式为"字tab拼音tab权重(可省略，默认为0)"。
- 您可能已经发现了，这就是RIME字典的格式！所以我们可以把RIME的字典删掉前面部分，然后后缀名改为.txt就能导入。不过我想默认字词库已经足够了。
------
- Q: Can I make my own skin?
- A: Of course! You just need to write a skin description and prepare a picture as the skin background. The format of the skin description is: skin title;skin description;button color rgba;main color 1 rgba;main color 2 rgba
- For example:
- HXIME white;author: HX2 xianglong90;(255,255,255,128);(52,161,255,255);(52,255,209,255)
- If you want to draw the skin accurately, you can find the ThemePSDInstruction.psd file and png file in the Themes folder to assist you in drawing!
- For the skin background, HXIME uses Texture2D instead of UISprite.
- That is to say, if you are willing to tinker, you can even try to load it online or use RenderTexture (that is, shoot it with the in-game camera)
- Q: How many sets of skins can be placed?
- A: Theoretically, it is basically unlimited. However, players currently only have slots for the first 9 skins. Of course, you can call the public method SetSkin(index) from the object with the HXIMEUI script installed to set the 10th and subsequent skins.
- Q: What if I want to import my own word library?
- A: PinyinEngine has word libraries for simplified and traditional Chinese. You can load a txt dictionary to replace the default word library. The format is "character tab pinyin tab weight (optional, default is 0)".
- You may have discovered that this is the format of the RIME dictionary! So we can delete the front part of the RIME dictionary, and then change the suffix to .txt to import it. But I think the default word library is enough.
------
- Q: 独自のスキンを作成できますか?
- A: もちろんです！スキンの説明を書いて、スキンの背景となる画像を用意するだけです。スキンの説明の形式は次のとおりです: スキンのタイトル;スキンの説明。ボタンの色 RGBA;メインカラー 1 rgba;メインカラー2 RGBA
- 例えば：
- HXIME ホワイト;著者: HX2 xianglong90;(255,255,255,128);(52,161,255,255);(52,255,209,255)
- スキンを正確に描きたい場合は、Themes フォルダー内の ThemePSDInstruction.psd ファイルと png ファイルを参照して描くこともできます。
- スキンの背景には、HXIME は UISprite ではなく Texture2D を使用します。
- つまり、いじる気があれば、オンラインでロードしたり、RenderTexture（つまり、ゲーム内のカメラで撮影する）を使用したりすることもできます。
- Q: スキンは何セットまで配置できますか?
- A: 理論上は基本的に無制限です。ただし、現在プレイヤーが選択できるスキンのスロットは最初の 9 つだけです。もちろん、HXIMEUI スクリプトがインストールされたオブジェクトからパブリック メソッド SetSkin(index) を呼び出して、10 番目以降のスキンを設定することもできます。
- Q: 独自の語彙をインポートしたい場合はどうすればいいですか?
- A: PinyinEngine には、簡体字中国語と繁体字中国語の両方の単語ライブラリが含まれています。 txt 辞書をロードして、デフォルトの単語ライブラリを置き換えることができます。形式は「文字タブ ピンインタブ 重み（省略可能、デフォルトは 0）」です。
- ご想像のとおり、これが RIME 辞書の形式です。したがって、RIME 辞書の前半部分を削除し、サフィックスを .txt に変更してインポートすることができます。しかし、デフォルトの単語ライブラリで十分だと思います。

# Licence: LGPL
https://github.com/xianglong90II/VRChatChineseIME_HXIME

# Credit:
- 图标 Icons　アイコン: Google Material UI & Fonts https://fonts.google.com/
- 字体 Font　フォント: Noto Sans  https://fonts.google.com/noto/specimen/Noto+Sans+SC?lang=zh_Hans
- 简体词库 Simplified Chinese Dictionary　簡体字辞書データ：RIME袖珍简化字拼音 https://github.com/rime/rime-pinyin-simp
- 繁体词库 Traditional Chinese Dictionary　繫体字辞書データ：RIME朙月拼音 https://github.com/rime/rime-luna-pinyin
