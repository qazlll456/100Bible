using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BiblePlugin
{
    public class LanguageManager
    {
        private readonly string _moduleDirectory;
        private readonly Dictionary<string, Language> _languageCache = new();
        public Language? Language { get; private set; }

        public LanguageManager(string moduleDirectory)
        {
            _moduleDirectory = moduleDirectory ?? throw new ArgumentNullException(nameof(moduleDirectory));
        }

        public bool LoadLanguage(string languageFile)
        {
            if (string.IsNullOrEmpty(languageFile))
            {
                Server.PrintToConsole("Error: Language file name is empty. Falling back to english.json.");
                languageFile = "english.json";
            }

            if (_languageCache.TryGetValue(languageFile, out var cachedLanguage))
            {
                Language = cachedLanguage;
                Server.PrintToConsole($"Loaded cached language: {languageFile}");
                return true;
            }

            var languagePath = Path.Combine(_moduleDirectory, "language", languageFile);
            if (!File.Exists(languagePath))
            {
                if (languageFile != "english.json")
                {
                    Server.PrintToConsole($"Error: Language file {languageFile} not found, falling back to english.json.");
                    return LoadLanguage("english.json");
                }
                GenerateDefaultLanguages();
            }

            try
            {
                var json = File.ReadAllText(languagePath);
                var language = JsonSerializer.Deserialize<Language>(json);
                if (language == null || string.IsNullOrEmpty(language.MessagePrefix) || language.Messages == null ||
                    language.Messages.Length == 0 || language.Messages.Any(m => m == null || m.Id < 1 || string.IsNullOrEmpty(m.Text)) ||
                    language.Messages.GroupBy(m => m.Id).Any(g => g.Count() > 1))
                {
                    Server.PrintToConsole($"Error: Invalid language file {languageFile}. Plugin stopped.");
                    return false;
                }

                Language = language;
                _languageCache[languageFile] = language;
                Server.PrintToConsole($"Loaded language: {languageFile}, {language.Messages.Length} messages.");
                return true;
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Error: Invalid JSON in {languageFile} ({ex.Message}). Plugin stopped.");
                return false;
            }
        }

        public Language? GetPlayerLanguage(string languageFile)
        {
            if (_languageCache.TryGetValue(languageFile, out var cachedLanguage))
            {
                return cachedLanguage;
            }
            return LoadLanguage(languageFile) ? _languageCache[languageFile] : null;
        }

        private void GenerateDefaultLanguages()
        {
            var languageDir = Path.Combine(_moduleDirectory, "language");
            try
            {
                Directory.CreateDirectory(languageDir);
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Error: Failed to create language directory ({ex.Message}).");
                return;
            }

            var english = new Language
            {
                MessagePrefix = "{green}100Bible {0}: {white}{1}",
                Messages = new Message[100]
            };

            var tChinese = new Language
            {
                MessagePrefix = "{blue}100Bible {0}: {white}{1}",
                Messages = new Message[100]
            };

            // Populate messages (100 verses)
            for (int i = 0; i < 100; i++)
            {
                int id = i + 1;
                string color = GetColorTag(id); // Rotate colors
                english.Messages[i] = new Message
                {
                    Id = id,
                    Text = $"{color}{GetEnglishVerse(id)}"
                };
                tChinese.Messages[i] = new Message
                {
                    Id = id,
                    Text = $"{color}{GetChineseVerse(id)}"
                };
            }

            try
            {
                File.WriteAllText(Path.Combine(languageDir, "english.json"), JsonSerializer.Serialize(english, new JsonSerializerOptions { WriteIndented = true }));
                File.WriteAllText(Path.Combine(languageDir, "t-chinese.json"), JsonSerializer.Serialize(tChinese, new JsonSerializerOptions { WriteIndented = true }));
                Server.PrintToConsole("Generated default english.json and t-chinese.json with 100 messages each.");
                LoadLanguage("english.json");
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Error: Failed to generate default languages ({ex.Message}).");
            }
        }

        private string GetColorTag(int id)
        {
            string[] colors = { "{white}", "{green}", "{yellow}", "{blue}", "{red}", "{cyan}", "{purple}" };
            return colors[id % colors.Length];
        }

        private string GetEnglishVerse(int id)
        {
            var verses = new Dictionary<int, string>
            {
                { 1, "For God so loved the world, that he gave his only Son, that whoever believes in him should not perish but have eternal life. - John 3:16 - KJV" },
                { 2, "The Lord is my shepherd; I shall not want. - Psalm 23:1 - KJV" },
                { 3, "I can do all things through Christ which strengtheneth me. - Philippians 4:13 - KJV" },
                { 4, "Be strong and courageous. Do not be afraid; do not be discouraged, for the Lord your God will be with you. - Joshua 1:9 - NIV" },
                { 5, "Trust in the Lord with all your heart and lean not on your own understanding. - Proverbs 3:5 - NIV" },
                { 6, "But seek first his kingdom and his righteousness, and all these things will be given to you as well. - Matthew 6:33 - NIV" },
                { 7, "For I know the plans I have for you, declares the Lord, plans to prosper you and not to harm you. - Jeremiah 29:11 - NIV" },
                { 8, "If we confess our sins, he is faithful and just and will forgive us our sins. - 1 John 1:9 - NIV" },
                { 9, "The Lord is my light and my salvation—whom shall I fear? - Psalm 27:1 - NIV" },
                { 10, "Do to others as you would have them do to you. - Luke 6:31 - NIV" },
                { 11, "In the beginning God created the heavens and the earth. - Genesis 1:1 - NIV" },
                { 12, "And we know that in all things God works for the good of those who love him. - Romans 8:28 - NIV" },
                { 13, "The fear of the Lord is the beginning of wisdom. - Proverbs 9:10 - NIV" },
                { 14, "Blessed are the peacemakers, for they will be called children of God. - Matthew 5:9 - NIV" },
                { 15, "Cast all your anxiety on him because he cares for you. - 1 Peter 5:7 - NIV" },
                { 16, "Delight yourself in the Lord, and he will give you the desires of your heart. - Psalm 37:4 - NIV" },
                { 17, "Come to me, all you who are weary and burdened, and I will give you rest. - Matthew 11:28 - NIV" },
                { 18, "The Lord is near to all who call on him, to all who call on him in truth. - Psalm 145:18 - NIV" },
                { 19, "For by grace you have been saved through faith. - Ephesians 2:8 - NIV" },
                { 20, "Let your light shine before others, that they may see your good deeds. - Matthew 5:16 - NIV" },
                { 21, "Ask and it will be given to you; seek and you will find. - Matthew 7:7 - NIV" },
                { 22, "The Lord is good, a refuge in times of trouble. - Nahum 1:7 - NIV" },
                { 23, "Do not be overcome by evil, but overcome evil with good. - Romans 12:21 - NIV" },
                { 24, "He gives strength to the weary and increases the power of the weak. - Isaiah 40:29 - NIV" },
                { 25, "Be kind and compassionate to one another, forgiving each other. - Ephesians 4:32 - NIV" },
                { 26, "The Lord is my rock, my fortress and my deliverer. - Psalm 18:2 - NIV" },
                { 27, "Whatever you do, work at it with all your heart, as working for the Lord. - Colossians 3:23 - NIV" },
                { 28, "Peace I leave with you; my peace I give you. - John 14:27 - NIV" },
                { 29, "The Lord will fight for you; you need only to be still. - Exodus 14:14 - NIV" },
                { 30, "Love your neighbor as yourself. - Mark 12:31 - NIV" },
                { 31, "The Lord is compassionate and gracious, slow to anger, abounding in love. - Psalm 103:8 - NIV" },
                { 32, "I am the way and the truth and the life. No one comes to the Father except through me. - John 14:6 - NIV" },
                { 33, "Give thanks to the Lord, for he is good; his love endures forever. - Psalm 107:1 - NIV" },
                { 34, "But the fruit of the Spirit is love, joy, peace, forbearance, kindness. - Galatians 5:22 - NIV" },
                { 35, "Wait for the Lord; be strong and take heart and wait for the Lord. - Psalm 27:14 - NIV" },
                { 36, "Do not let your hearts be troubled. You believe in God; believe also in me. - John 14:1 - NIV" },
                { 37, "The Lord is close to the brokenhearted and saves those who are crushed in spirit. - Psalm 34:18 - NIV" },
                { 38, "For the word of God is alive and active. - Hebrews 4:12 - NIV" },
                { 39, "Be joyful in hope, patient in affliction, faithful in prayer. - Romans 12:12 - NIV" },
                { 40, "The Lord your God is with you, the Mighty Warrior who saves. - Zephaniah 3:17 - NIV" },
                { 41, "So do not fear, for I am with you; do not be dismayed, for I am your God. - Isaiah 41:10 - NIV" },
                { 42, "Rejoice always, pray continually, give thanks in all circumstances. - 1 Thessalonians 5:16-18 - NIV" },
                { 43, "The name of the Lord is a fortified tower; the righteous run to it and are safe. - Proverbs 18:10 - NIV" },
                { 44, "God is our refuge and strength, an ever-present help in trouble. - Psalm 46:1 - NIV" },
                { 45, "For where two or three gather in my name, there am I with them. - Matthew 18:20 - NIV" },
                { 46, "The Lord is faithful to all his promises and loving toward all he has made. - Psalm 145:13 - NIV" },
                { 47, "But those who hope in the Lord will renew their strength. - Isaiah 40:31 - NIV" },
                { 48, "Above all, love each other deeply, because love covers over a multitude of sins. - 1 Peter 4:8 - NIV" },
                { 49, "The Lord is my strength and my shield; my heart trusts in him. - Psalm 28:7 - NIV" },
                { 50, "Therefore, if anyone is in Christ, the new creation has come. - 2 Corinthians 5:17 - NIV" },
                { 51, "Taste and see that the Lord is good; blessed is the one who takes refuge in him. - Psalm 34:8 - NIV" },
                { 52, "And my God will meet all your needs according to the riches of his glory in Christ Jesus. - Philippians 4:19 - NIV" },
                { 53, "The Lord is gracious and righteous; our God is full of compassion. - Psalm 116:5 - NIV" },
                { 54, "If God is for us, who can be against us? - Romans 8:31 - NIV" },
                { 55, "Your word is a lamp for my feet, a light on my path. - Psalm 119:105 - NIV" },
                { 56, "Let the peace of Christ rule in your hearts. - Colossians 3:15 - NIV" },
                { 57, "The Lord is not slow in keeping his promise, as some understand slowness. - 2 Peter 3:9 - NIV" },
                { 58, "A friend loves at all times, and a brother is born for a time of adversity. - Proverbs 17:17 - NIV" },
                { 59, "I have come that they may have life, and have it to the full. - John 10:10 - NIV" },
                { 60, "The Lord is my helper; I will not be afraid. What can mere mortals do to me? - Hebrews 13:6 - NIV" },
                { 61, "Blessed is the one who does not walk in step with the wicked. - Psalm 1:1 - NIV" },
                { 62, "For we live by faith, not by sight. - 2 Corinthians 5:7 - NIV" },
                { 63, "The heavens declare the glory of God; the skies proclaim the work of his hands. - Psalm 19:1 - NIV" },
                { 64, "Greater love has no one than this: to lay down one’s life for one’s friends. - John 15:13 - NIV" },
                { 65, "Commit to the Lord whatever you do, and he will establish your plans. - Proverbs 16:3 - NIV" },
                { 66, "The Lord is righteous in all his ways and faithful in all he does. - Psalm 145:17 - NIV" },
                { 67, "Do everything in love. - 1 Corinthians 16:14 - NIV" },
                { 68, "The Lord is good to all; he has compassion on all he has made. - Psalm 145:9 - NIV" },
                { 69, "For the Lord is good and his love endures forever; his faithfulness continues through all generations. - Psalm 100:5 - NIV" },
                { 70, "You are my hiding place; you will protect me from trouble. - Psalm 32:7 - NIV" },
                { 71, "The Lord is my portion; therefore I will wait for him. - Lamentations 3:24 - NIV" },
                { 72, "For everyone who asks receives; the one who seeks finds. - Luke 11:10 - NIV" },
                { 73, "The Lord is a refuge for the oppressed, a stronghold in times of trouble. - Psalm 9:9 - NIV" },
                { 74, "My help comes from the Lord, the Maker of heaven and earth. - Psalm 121:2 - NIV" },
                { 75, "Whoever dwells in the shelter of the Most High will rest in the shadow of the Almighty. - Psalm 91:1 - NIV" },
                { 76, "The Lord your God is God; he is the faithful God, keeping his covenant of love. - Deuteronomy 7:9 - NIV" },
                { 77, "Be still, and know that I am God. - Psalm 46:10 - NIV" },
                { 78, "He heals the brokenhearted and binds up their wounds. - Psalm 147:3 - NIV" },
                { 79, "The Lord is my strength and my song; he has become my salvation. - Exodus 15:2 - NIV" },
                { 80, "Let us not become weary in doing good, for at the proper time we will reap a harvest. - Galatians 6:9 - NIV" },
                { 81, "The Lord is king forever and ever; the nations will perish from his land. - Psalm 10:16 - NIV" },
                { 82, "For the eyes of the Lord are on the righteous, and his ears are attentive to their prayer. - 1 Peter 3:12 - NIV" },
                { 83, "The Lord is patient with you, not wanting anyone to perish. - 2 Peter 3:9 - NIV" },
                { 84, "I sought the Lord, and he answered me; he delivered me from all my fears. - Psalm 34:4 - NIV" },
                { 85, "The Lord is holy; he is exalted above all the nations. - Psalm 99:5 - NIV" },
                { 86, "But you, Lord, are a compassionate and gracious God, slow to anger. - Psalm 86:15 - NIV" },
                { 87, "The Lord is my strength and my defense; he has become my salvation. - Psalm 118:14 - NIV" },
                { 88, "For the Lord gives wisdom; from his mouth come knowledge and understanding. - Proverbs 2:6 - NIV" },
                { 89, "The Lord is merciful and gracious, slow to anger and abounding in steadfast love. - Psalm 103:8 - NIV" },
                { 90, "You will keep in perfect peace those whose minds are steadfast. - Isaiah 26:3 - NIV" },
                { 91, "The Lord is upright; he is my Rock, and there is no wickedness in him. - Psalm 92:15 - NIV" },
                { 92, "Every word of God is flawless; he is a shield to those who take refuge in him. - Proverbs 30:5 - NIV" },
                { 93, "The Lord is good to those whose hope is in him, to the one who seeks him. - Lamentations 3:25 - NIV" },
                { 94, "For the Lord loves the just and will not forsake his faithful ones. - Psalm 37:28 - NIV" },
                { 95, "The Lord watches over the foreigner and sustains the fatherless and the widow. - Psalm 146:9 - NIV" },
                { 96, "The Lord is trustworthy in all he promises and faithful in all he does. - Psalm 145:13 - NIV" },
                { 97, "The Lord is exalted, for he dwells on high; he will fill Zion with his justice. - Isaiah 33:5 - NIV" },
                { 98, "The Lord is my banner, for it is his right hand that has worked salvation for me. - Exodus 17:15 - NIV" },
                { 99, "The Lord is righteous; he has cut me free from the cords of the wicked. - Psalm 129:4 - NIV" },
                { 100, "Give thanks to the Lord, for he is good; his love endures forever. - Psalm 118:1 - NIV" }
            };

            return verses[id];
        }

        private string GetChineseVerse(int id)
        {
            var verses = new Dictionary<int, string>
            {
                { 1, "神愛世人，甚至將他的獨生子賜給他們，叫一切信他的，不至滅亡，反得永生。 - 約翰福音 3:16 - 和合本" },
                { 2, "耶和華是我的牧者，我必不致缺乏。 - 詩篇 23:1 - 和合本" },
                { 3, "我靠著那加給我力量的，凡事都能做。 - 腓立比書 4:13 - 和合本" },
                { 4, "你要剛強壯膽！不要懼怕，也不要驚惶，因為耶和華你的神與你同在。 - 約書亞記 1:9 - 和合本" },
                { 5, "你要專心仰賴耶和華，不可倚靠自己的聰明。 - 箴言 3:5 - 和合本" },
                { 6, "你們要先求他的國和他的義，這些東西都要加給你們了。 - 馬太福音 6:33 - 和合本" },
                { 7, "耶和華說：我向你們所懷的意念是賜平安的意念，不是降災禍的意念。 - 耶利米書 29:11 - 和合本" },
                { 8, "我們若認自己的罪，神是信實的，是公義的，必要赦免我們的罪。 - 約翰一書 1:9 - 和合本" },
                { 9, "耶和華是我的亮光，是我的救恩，我還怕誰呢？ - 詩篇 27:1 - 和合本" },
                { 10, "你們願意人怎樣待你們，你們也要怎樣待人。 - 路加福音 6:31 - 和合本" },
                { 11, "起初，神創造天地。 - 創世記 1:1 - 和合本" },
                { 12, "我們曉得萬事都互相效力，叫愛神的人得益處。 - 羅馬書 8:28 - 和合本" },
                { 13, "敬畏耶和華是智慧的開端。 - 箴言 9:10 - 和合本" },
                { 14, "使人和睦的人有福了！因為他們必稱為神的兒子。 - 馬太福音 5:9 - 和合本" },
                { 15, "你們要將一切的憂慮卸給神，因為他顧念你們。 - 彼得前書 5:7 - 和合本" },
                { 16, "以耶和華為樂，他就將你心裡所求的賜給你。 - 詩篇 37:4 - 和合本" },
                { 17, "凡勞苦擔重擔的人可以到我這裡來，我就使你們得安息。 - 馬太福音 11:28 - 和合本" },
                { 18, "凡真心求告耶和華的，耶和華便與他們相近。 - 詩篇 145:18 - 和合本" },
                { 19, "你們得救是本乎恩，也因著信。 - 以弗所書 2:8 - 和合本" },
                { 20, "你們的光也當這樣照在人前，叫他們看見你們的好行為。 - 馬太福音 5:16 - 和合本" },
                { 21, "你們祈求，就給你們；尋找，就尋見。 - 馬太福音 7:7 - 和合本" },
                { 22, "耶和華本為善，在患難之日為人的避難所。 - 那鴻書 1:7 - 和合本" },
                { 23, "不可被惡所勝，反要以善勝惡。 - 羅馬書 12:21 - 和合本" },
                { 24, "他使疲乏的有力氣，使軟弱的有能力。 - 以賽亞書 40:29 - 和合本" },
                { 25, "並要以恩慈相待，存憐憫的心，彼此饒恕。 - 以弗所書 4:32 - 和合本" },
                { 26, "耶和華是我的磐石，我的山寨，我的救主。 - 詩篇 18:2 - 和合本" },
                { 27, "凡你手所當做的事，要盡心去做，因為是為主做的。 - 歌羅西書 3:23 - 和合本" },
                { 28, "我留下平安給你們，我將我的平安賜給你們。 - 約翰福音 14:27 - 和合本" },
                { 29, "耶和華要為你爭戰，你只管靜默！ - 出埃及記 14:14 - 和合本" },
                { 30, "要愛你的鄰舍，如同自己。 - 馬可福音 12:31 - 和合本" },
                { 31, "耶和華有憐憫，有恩典，不輕易發怒，且有豐盛的慈愛。 - 詩篇 103:8 - 和合本" },
                { 32, "我就是道路、真理、生命；若不藉著我，沒有人能到父那裡去。 - 約翰福音 14:6 - 和合本" },
                { 33, "當稱謝耶和華，因他本為善，他的慈愛永遠長存。 - 詩篇 107:1 - 和合本" },
                { 34, "聖靈所結的果子，就是仁愛、喜樂、和平、忍耐、恩慈。 - 加拉太書 5:22 - 和合本" },
                { 35, "要等候耶和華，當壯膽，堅固你的心，等候耶和華！ - 詩篇 27:14 - 和合本" },
                { 36, "你們心裡不要憂愁，你們信神，也當信我。 - 約翰福音 14:1 - 和合本" },
                { 37, "耶和華靠近傷心的人，拯救靈性痛悔的人。 - 詩篇 34:18 - 和合本" },
                { 38, "神的道是活潑的，是有功效的。 - 希伯來書 4:12 - 和合本" },
                { 39, "在指望中要喜樂，在患難中要忍耐，禱告要恆切。 - 羅馬書 12:12 - 和合本" },
                { 40, "耶和華你的神在你中間，是施行拯救的大能者。 - 西番雅書 3:17 - 和合本" },
                { 41, "不要懼怕，因為我與你同在；不要驚惶，因為我是你的神。 - 以賽亞書 41:10 - 和合本" },
                { 42, "要常常喜樂，不住地禱告，凡事謝恩。 - 帖撒羅尼迦前書 5:16-18 - 和合本" },
                { 43, "耶和華的名是堅固台，義人奔入便得安穩。 - 箴言 18:10 - 和合本" },
                { 44, "神是我們的避難所，是我們的力量，是我們在患難中隨時的幫助。 - 詩篇 46:1 - 和合本" },
                { 45, "因為有兩三個人奉我的名聚會，那裡就有我在他們中間。 - 馬太福音 18:20 - 和合本" },
                { 46, "耶和華向許願的守信，對凡他所造的施慈愛。 - 詩篇 145:13 - 和合本" },
                { 47, "但那等候耶和華的，必重新得力。 - 以賽亞書 40:31 - 和合本" },
                { 48, "最要緊的是彼此切實相愛，因為愛能遮掩許多的罪。 - 彼得前書 4:8 - 和合本" },
                { 49, "耶和華是我的力量，是我的盾牌，我心裡倚靠他。 - 詩篇 28:7 - 和合本" },
                { 50, "若有人在基督裡，他就是新造的人，舊事已過，都變成新的了。 - 哥林多後書 5:17 - 和合本" },
                { 51, "你們要嘗嘗主恩的滋味，便知道他是美善；投靠他的人有福了！ - 詩篇 34:8 - 和合本" },
                { 52, "我的神必照他榮耀的豐富，在基督耶穌裡使你們一切所需用的都充足。 - 腓立比書 4:19 - 和合本" },
                { 53, "耶和華有恩惠，有公義，我們的神滿有憐憫。 - 詩篇 116:5 - 和合本" },
                { 54, "神若幫助我們，誰能敵擋我們呢？ - 羅馬書 8:31 - 和合本" },
                { 55, "你的話是我腳前的燈，是我路上的光。 - 詩篇 119:105 - 和合本" },
                { 56, "當叫基督的平安在你們心裡作主。 - 歌羅西書 3:15 - 和合本" },
                { 57, "主耶和華不像人看為遲延，他乃是寬容你們。 - 彼得後書 3:9 - 和合本" },
                { 58, "朋友乃時常親愛，弟兄為患難而生。 - 箴言 17:17 - 和合本" },
                { 59, "我來了，是要叫人得生命，並且得的更豐盛。 - 約翰福音 10:10 - 和合本" },
                { 60, "耶和華是我的幫助，我必不懼怕，人能把我怎麼樣呢？ - 希伯來書 13:6 - 和合本" },
                { 61, "不與惡人同行的義人有福了！ - 詩篇 1:1 - 和合本" },
                { 62, "因為我們行事為人，是憑著信心，不是憑著眼見。 - 哥林多後書 5:7 - 和合本" },
                { 63, "諸天述說神的榮耀，穹蒼傳揚他的手段。 - 詩篇 19:1 - 和合本" },
                { 64, "人為朋友捨命，人的愛心沒有比這個大的。 - 約翰福音 15:13 - 和合本" },
                { 65, "凡你手所當做的事要交託耶和華，你的籌算就必成立。 - 箴言 16:3 - 和合本" },
                { 66, "耶和華在一切所行的無不公義，所做的盡都誠實。 - 詩篇 145:17 - 和合本" },
                { 67, "凡事都要憑愛心而行。 - 哥林多前書 16:14 - 和合本" },
                { 68, "耶和華善待萬民，他的慈悲覆庇他一切所造的。 - 詩篇 145:9 - 和合本" },
                { 69, "因耶和華本為善，他的慈愛存到永遠，他的信實直到萬代。 - 詩篇 100:5 - 和合本" },
                { 70, "你是我的藏身之處，你必保護我脫離苦難。 - 詩篇 32:7 - 和合本" },
                { 71, "耶和華是我的業分，因此我要等候他。 - 耶利米哀歌 3:24 - 和合本" },
                { 72, "因為凡祈求的，就得著；尋找的，就尋見。 - 路加福音 11:10 - 和合本" },
                { 73, "耶和華是受欺壓者的避難所，是患難中的避難所。 - 詩篇 9:9 - 和合本" },
                { 74, "我的幫助從造天地的耶和華而來。 - 詩篇 121:2 - 和合本" },
                { 75, "凡住在至高者隱密處的，必住在全能者的蔭下。 - 詩篇 91:1 - 和合本" },
                { 76, "耶和華你的神是真神，是信實的神，向愛他的人守約施慈愛。 - 申命記 7:9 - 和合本" },
                { 77, "你們要休息，要知道我是神！ - 詩篇 46:10 - 和合本" },
                { 78, "他醫好傷心的人，裹好他們的傷處。 - 詩篇 147:3 - 和合本" },
                { 79, "耶和華是我的力量，是我的詩歌，他也成了我的救恩。 - 出埃及記 15:2 - 和合本" },
                { 80, "行善不可喪志，若不灰心，到了時候就要收成。 - 加拉太書 6:9 - 和合本" },
                { 81, "耶和華永遠作王，列國從他的地上滅絕。 - 詩篇 10:16 - 和合本" },
                { 82, "因為主的眼看顧義人，主的耳聽他們的祈禱。 - 彼得前書 3:12 - 和合本" },
                { 83, "主耶和華寬容，不願一人沉淪。 - 彼得後書 3:9 - 和合本" },
                { 84, "我尋求耶和華，他就應允我，救我脫離一切的恐懼。 - 詩篇 34:4 - 和合本" },
                { 85, "耶和華是聖潔的，高於萬民之上。 - 詩篇 99:5 - 和合本" },
                { 86, "主耶和華，你是有憐憫有恩典的神，不輕易發怒。 - 詩篇 86:15 - 和合本" },
                { 87, "耶和華是我的力量，是我的保障，他成了我的救恩。 - 詩篇 118:14 - 和合本" },
                { 88, "因為耶和華賜智慧，知識和聰明都由他口而出。 - 箴言 2:6 - 和合本" },
                { 89, "耶和華有恩惠有憐憫，不輕易發怒，且有豐盛的慈愛。 - 詩篇 103:8 - 和合本" },
                { 90, "你必使心裡堅定的人得享平安。 - 以賽亞書 26:3 - 和合本" },
                { 91, "耶和華是正直的，他是我的磐石，在他毫無不義。 - 詩篇 92:15 - 和合本" },
                { 92, "神的言語句句都是煉淨的，凡投靠他的，他便作他們的盾牌。 - 箴言 30:5 - 和合本" },
                { 93, "耶和華善待凡仰望他的人，尋求他的人。 - 耶利米哀歌 3:25 - 和合本" },
                { 94, "因為耶和華喜愛公平，不撇棄他的聖民。 - 詩篇 37:28 - 和合本" },
                { 95, "耶和華眷顧寄居的，扶持孤兒和寡婦。 - 詩篇 146:9 - 和合本" },
                { 96, "耶和華凡應許的都守信，凡他所做的都施慈愛。 - 詩篇 145:13 - 和合本" },
                { 97, "耶和華高高在上，用公義充滿錫安。 - 以賽亞書 33:5 - 和合本" },
                { 98, "耶和華是我的旌旗，因他的右手為我施行救恩。 - 出埃及記 17:15 - 和合本" },
                { 99, "耶和華是公義的，他砍斷了惡人的繩索。 - 詩篇 129:4 - 和合本" },
                { 100, "你們要稱謝耶和華，因他本為善，他的慈愛永遠長存。 - 詩篇 118:1 - 和合本" }
            };

            return verses[id];
        }
    }

    public class Language
    {
        public string MessagePrefix { get; set; } = string.Empty;
        public Message[] Messages { get; set; } = Array.Empty<Message>();
    }

    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}