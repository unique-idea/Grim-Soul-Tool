using Engine.Enums;
using Engine.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Engine.References.Models
{
    public static class ItemsRegistry
    {
        public static List<Item> GetDefaultItems()
        {
            return new List<Item>
            {
                new Item
                {
                    Name = ItemName.IronGrate,
                    Type = ItemType.Resource,
                    DisplayName = "Iron Grate",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#54444C"),
                        ColorTranslator.FromHtml("#2B2C2B"),
                        ColorTranslator.FromHtml("#1F1F1F"),
                        ColorTranslator.FromHtml("#171717"),
                        ColorTranslator.FromHtml("#151515")
                    },
                    // Đặc trưng của Vỉ Sắt: Ánh tím/lam trên thanh lưới (B-G >= 7, R-G >= 7, Total >= 180)
                    CustomEvaluator = pixels =>
                    {
                        int tanRope = pixels.Count(p => p.R > p.G && p.G > p.B && (p.R - p.G >= 10) && (p.G - p.B >= 10));
                        if (tanRope >= 80) return 0.0;

                        int bdMauve = pixels.Count(p => p.R >= 115 && p.R <= 155 && p.G >= 98 && p.G <= 135 && p.B >= 115 && p.B <= 155 && (p.R - p.G >= 13) && (p.B - p.G >= 10) && Math.Abs(p.R - p.B) <= 8);
                        if (bdMauve >= 100) return 0.0;

                        int lsCoolRim = pixels.Count(p => p.R >= 36 && p.R <= 62 && Math.Abs(p.R - p.G) <= 2 && (p.B - p.G >= 5) && (p.B - p.G <= 12));
                        if (lsCoolRim >= 100) return 0.0;

                        int meatRuby = pixels.Count(p => p.R >= 100 && p.R <= 220 && p.G >= 25 && p.G <= 105 && p.B >= 32 && p.B <= 118 && (p.R - p.G >= 68) && (p.R - p.B >= 55) && (p.B - p.G >= 4));
                        if (meatRuby >= 100) return 0.0;

                        int purpleCount = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        return purpleCount >= 50 ? (10000.0 + purpleCount) : 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Wire,
                    Type = ItemType.Resource,
                    DisplayName = "Wire",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#131313"),
                        ColorTranslator.FromHtml("#111111"),
                        ColorTranslator.FromHtml("#0B0B0B"),
                        ColorTranslator.FromHtml("#0A0A0A")
                    },
                    // Đặc trưng của Dây Kẽm: Búi cuộn dây đồng rất dày đặc (R-G >= 10 có > 700 px),
                    // khe bóng tối đen sâu giữa các vòng cuộn (Total <= 50, G - B <= 3, R >= G >= 600 px)
                    // và hầu như không có rỉ sắt ngả lục (olive rust: G - B >= 4, <= 150 px), không có ánh tím của Vỉ Sắt, không có màu tan của Bowstring.
                    CustomEvaluator = pixels =>
                    {
                        int purpleCount = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleCount >= 20) return 0.0;

                        int tanRope = pixels.Count(p => p.R > p.G && p.G > p.B && (p.R - p.G >= 10) && (p.G - p.B >= 10));
                        if (tanRope >= 80) return 0.0;

                        int copper = pixels.Count(p => (p.R - p.G >= 10));
                        if (copper < 400) return 0.0;

                        int wireDarkBundle = pixels.Count(p => (p.R + p.G + p.B <= 50) && (p.G - p.B <= 3) && (p.R >= p.G));
                        int chainOliveRust = pixels.Count(p => (p.G - p.B >= 4));

                        if (wireDarkBundle >= 600 && chainOliveRust <= 150)
                        {
                            return 10000.0 + (wireDarkBundle * 2.0) - (chainOliveRust * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Chain,
                    Type = ItemType.Resource,
                    DisplayName = "Chain",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#322A2A"),
                        ColorTranslator.FromHtml("#362E2E"),
                        ColorTranslator.FromHtml("#332B2B"),
                        ColorTranslator.FromHtml("#272727"),
                        ColorTranslator.FromHtml("#080808")
                    },
                    // Đặc trưng của Xích Sắt: Các mắt xích sắt dày có vệt rỉ ngả lục/ô-liu (G - B >= 4, >= 180 px)
                    // hoặc thân mắt xích xám tối (Total 35..55, |R-G| <= 2, |G-B| <= 2), bóng tối cuộn (wireDarkBundle) < 600 px, không có ánh tím của Vỉ Sắt, không có màu tan của Bowstring.
                    CustomEvaluator = pixels =>
                    {
                        int purpleCount = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleCount >= 20) return 0.0;

                        int tanRope = pixels.Count(p => p.R > p.G && p.G > p.B && (p.R - p.G >= 10) && (p.G - p.B >= 10));
                        if (tanRope >= 80) return 0.0;

                        int copper = pixels.Count(p => (p.R - p.G >= 7));
                        if (copper < 200) return 0.0;

                        int chainOliveRust = pixels.Count(p => (p.G - p.B >= 4));
                        int chainIronGray = pixels.Count(p => (p.R + p.G + p.B >= 35) && (p.R + p.G + p.B <= 55) && Math.Abs(p.R - p.G) <= 2 && Math.Abs(p.G - p.B) <= 2);
                        int wireDarkBundle = pixels.Count(p => (p.R + p.G + p.B <= 50) && (p.G - p.B <= 3) && (p.R >= p.G));

                        if (chainOliveRust >= 180 || (chainIronGray >= 150 && wireDarkBundle < 600))
                        {
                            return 10000.0 + (chainOliveRust * 3.0) + (chainIronGray * 1.5);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Bowstring,
                    Type = ItemType.Resource,
                    DisplayName = "Bowstring",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#5D4C3B"),
                        ColorTranslator.FromHtml("#6F5E4D"),
                        ColorTranslator.FromHtml("#7B6251"),
                        ColorTranslator.FromHtml("#5A4938"),
                        ColorTranslator.FromHtml("#3F3021"),
                        ColorTranslator.FromHtml("#5B4253")
                    },
                    // Đặc trưng của Dây Cung (Bowstring): Sợi dây thừng màu nâu vàng (tan rope: R > G > B, R-G >= 10, G-B >= 10)
                    // xuất hiện dày đặc >= 120 px ở mọi ô (các item khác chỉ có <= 48 px), kết hợp warmRope >= 180 px.
                    // Nút thắt bọc chỉ tím nhẹ (purpleGrate < 100 px, khác hẳn Vỉ Sắt > 600 px).
                    // Không có ánh vàng đồng thau dày đặc của Ghim Fibula (bronzeGold < 400, goldHighlight < 150).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int purpleGrate = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleGrate >= 100) return 0.0;

                        int bronzeGold = pixels.Count(p => p.R > p.G && p.G > p.B && (p.R - p.G >= 20) && (p.G - p.B >= 15) && (p.R + p.G + p.B >= 220));
                        int goldHighlight = pixels.Count(p => p.R >= 130 && p.G >= 95 && p.B >= 55 && (p.R - p.G >= 20) && (p.G - p.B >= 20) && (p.R + p.G + p.B >= 300));
                        int skBonePale = pixels.Count(p => p.R >= 185 && p.G >= 175 && p.B >= 155 && (p.R - p.G >= 4) && (p.R - p.G <= 16) && (p.G - p.B >= 7) && (p.G - p.B <= 26) && (p.R + p.G + p.B >= 520));
                        int aleMugDark = pixels.Count(p => p.R >= 55 && p.R <= 96 && p.G >= 29 && p.G <= 55 && p.B >= 29 && p.B <= 55 && Math.Abs(p.G - p.B) <= 3 && (p.R - p.G >= 26) && (p.R - p.G <= 45));
                        if (bronzeGold >= 400 || goldHighlight >= 150 || skBonePale >= 150 || aleMugDark >= 450) return 0.0;

                        int tanRope = pixels.Count(p => p.R > p.G && p.G > p.B && (p.R - p.G >= 10) && (p.G - p.B >= 10));
                        int warmRope = pixels.Count(p => p.R >= p.G + 8 && p.G >= p.B + 5 && (p.R + p.G + p.B >= 150));

                        if (tanRope >= 120 && warmRope >= 180)
                        {
                            return 10000.0 + (tanRope * 3.0) + warmRope;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Fibula,
                    Type = ItemType.Special,
                    DisplayName = "Fibula",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#7D5B4A"),
                        ColorTranslator.FromHtml("#775544"),
                        ColorTranslator.FromHtml("#664422"),
                        ColorTranslator.FromHtml("#DDCC88"),
                        ColorTranslator.FromHtml("#9F6C4A"),
                        ColorTranslator.FromHtml("#60472E"),
                        ColorTranslator.FromHtml("#936A41")
                    },
                    // Đặc trưng của Ghim Cài Áo (Fibula): Thân ghim bằng đồng đỏ sẫm (fibDarkBronze: R[95..135], G[60..95], B[30..75] >= 240 px, trung bình 500-760 px).
                    // Tỷ lệ fibDarkBronze >= ringGoldBody * 1.5 để phân biệt tuyệt đối với Nhẫn Vàng GoldRing.
                    // Không có ánh tím Vỉ Sắt (purpleGrate < 100 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int purpleGrate = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleGrate >= 100) return 0.0;

                        int fibDarkBronze = pixels.Count(p => (p.R >= 95 && p.R <= 135) && (p.G >= 60 && p.G <= 95) && (p.B >= 30 && p.B <= 75) && (p.R - p.G >= 25) && (p.G - p.B >= 15));
                        int ringGoldBody = pixels.Count(p => (p.R >= 145 && p.R <= 165) && (p.G >= 105 && p.G <= 125) && (p.B >= 65 && p.B <= 80) && (p.R - p.G >= 35) && (p.G - p.B >= 35));
                        int goldHighlight = pixels.Count(p => p.R >= 130 && p.G >= 95 && p.B >= 55 && (p.R - p.G >= 20) && (p.G - p.B >= 20) && (p.R + p.G + p.B >= 300));
                        int clothLinen = pixels.Count(p => p.R >= 145 && p.R <= 185 && p.G >= 95 && p.G <= 135 && p.B >= 65 && p.B <= 95 && (p.R - p.G >= 40) && (p.R - p.G <= 58) && (p.G - p.B >= 20) && (p.G - p.B <= 36));
                        int clothPeach = pixels.Count(p => p.R >= 120 && p.R <= 185 && (p.R - p.G >= 42) && (p.G - p.B >= 15) && (p.G - p.B <= 33));
                        int ropeMid = pixels.Count(p => p.R >= 115 && p.R <= 155 && p.G >= 90 && p.G <= 130 && p.B >= 68 && p.B <= 98 && (p.R - p.G >= 20) && (p.R - p.G <= 29) && (p.G - p.B >= 20) && (p.G - p.B <= 36));
                        int ropeDark = pixels.Count(p => p.R >= 85 && p.R <= 115 && p.G >= 60 && p.G <= 88 && p.B >= 45 && p.B <= 72 && (p.R - p.G >= 20) && (p.R - p.G <= 28) && (p.G - p.B >= 12) && (p.G - p.B <= 22));
                        int hbMahogany = pixels.Count(p => p.R >= 65 && p.R <= 92 && p.G >= 35 && p.G <= 58 && p.B >= 42 && p.B <= 66 && (p.R - p.G >= 25) && (p.B - p.G >= 3));
                        int pmpCream = pixels.Count(p => p.R >= 205 && p.G >= 172 && p.B >= 130 && (p.R - p.G >= 15) && (p.R - p.G <= 45) && (p.G - p.B >= 20) && (p.G - p.B <= 52));
                        int pmpIvory = pixels.Count(p => p.R >= 210 && p.G >= 195 && p.B >= 160 && (p.R - p.G >= 4) && (p.G - p.B >= 18) && (p.R + p.G + p.B >= 565));
                        int cndHighlight = pixels.Count(p => p.R >= 165 && p.R <= 230 && p.G >= 125 && p.G <= 190 && p.B >= 70 && p.B <= 125 && (p.R - p.G >= 28) && (p.R - p.G <= 52) && (p.G - p.B >= 48));
                        int flxStrand = pixels.Count(p => p.R >= 145 && p.R <= 182 && p.G >= 112 && p.G <= 148 && p.B >= 78 && p.B <= 114 && (p.R - p.G >= 28) && (p.R - p.G <= 38) && (p.G - p.B >= 28) && (p.G - p.B <= 38));

                        if (clothPeach < 250 && clothLinen < 200 && ropeMid < 120 && ropeDark < 120 && hbMahogany < 45 && pmpCream < 250 && pmpIvory < 120 && cndHighlight < 320 && flxStrand < 120 && fibDarkBronze >= 240 && fibDarkBronze >= ringGoldBody * 1.5 && goldHighlight >= 200)
                        {
                            return 10000.0 + (fibDarkBronze * 3.0) + goldHighlight;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.GoldRing,
                    Type = ItemType.Special,
                    DisplayName = "Gold Ring",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#9A7148"),
                        ColorTranslator.FromHtml("#8E5433"),
                        ColorTranslator.FromHtml("#8E5634"),
                        ColorTranslator.FromHtml("#8D5A38"),
                        ColorTranslator.FromHtml("#AB8A58"),
                        ColorTranslator.FromHtml("#8B5836")
                    },
                    // Đặc trưng của Nhẫn Vàng (Gold Ring): Thân nhẫn vàng rực sáng (ringGoldBody: R[145..165], G[105..125], B[65..80] >= 180 px, trung bình 530-580 px),
                    // dải phản quang màu cam đồng (ringCopBand: R[135..148], G[80..95], B[45..60] đạt 360-390 px).
                    // fibDarkBronze <= 200 px và ringGoldBody >= fibDarkBronze * 1.5 loại trừ hoàn toàn Fibula (Fibula có fibDarkBronze >= 500-760 px).
                    // Không có ánh tím Vỉ Sắt (purpleGrate < 100 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int purpleGrate = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleGrate >= 100) return 0.0;

                        int ringCopBand = pixels.Count(p => (p.R >= 135 && p.R <= 148) && (p.G >= 80 && p.G <= 95) && (p.B >= 45 && p.B <= 60) && (p.R - p.G >= 48));
                        int ringGoldBody = pixels.Count(p => (p.R >= 145 && p.R <= 165) && (p.G >= 105 && p.G <= 125) && (p.B >= 65 && p.B <= 80) && (p.R - p.G >= 35) && (p.G - p.B >= 35));
                        int fibDarkBronze = pixels.Count(p => (p.R >= 95 && p.R <= 135) && (p.G >= 60 && p.G <= 95) && (p.B >= 30 && p.B <= 75) && (p.R - p.G >= 25) && (p.G - p.B >= 15));

                        if (ringGoldBody >= 180 && fibDarkBronze <= 200 && ringGoldBody >= fibDarkBronze * 1.5)
                        {
                            return 10000.0 + (ringGoldBody * 4.0) + ringCopBand;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Lock,
                    Type = ItemType.Resource,
                    DisplayName = "Lock",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#4A2817"),
                        ColorTranslator.FromHtml("#69402F"),
                        ColorTranslator.FromHtml("#6A4130"),
                        ColorTranslator.FromHtml("#6D4433"),
                        ColorTranslator.FromHtml("#6C4332"),
                        ColorTranslator.FromHtml("#754C3B"),
                        ColorTranslator.FromHtml("#302018"),
                        ColorTranslator.FromHtml("#54524A")
                    },
                    // Đặc trưng của Ổ Khóa (Lock): Thân khóa bằng đồng nâu sẫm (lockBrownBody: R[65..120], G[35..75], B[20..55], R-G >= 20, G-B >= 10 đạt >= 500 px),
                    // càng khóa kim loại xám bạc (steelShackle: R,G,B[60..155], |R-G| <= 12, |G-B| <= 12, Total >= 200 đạt >= 150 px).
                    // Không có điểm sáng vàng chói của Fibula/GoldRing (goldHighlight < 120 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int lockBrownBody = pixels.Count(p => p.R >= 65 && p.R <= 120 && p.G >= 35 && p.G <= 75 && p.B >= 20 && p.B <= 55 && (p.R - p.G >= 20) && (p.G - p.B >= 10));
                        int steelShackle = pixels.Count(p => p.R >= 60 && p.R <= 150 && p.G >= 60 && p.G <= 150 && p.B >= 60 && p.B <= 155 && Math.Abs(p.R - p.G) <= 12 && Math.Abs(p.G - p.B) <= 12 && (p.R + p.G + p.B >= 200));
                        int goldHighlight = pixels.Count(p => p.R >= 130 && p.G >= 95 && p.B >= 55 && (p.R - p.G >= 20) && (p.G - p.B >= 20) && (p.R + p.G + p.B >= 300));

                        if (lockBrownBody >= 500 && steelShackle >= 150 && goldHighlight < 120)
                        {
                            return 10000.0 + (lockBrownBody * 2.0) + (steelShackle * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Cords,
                    Type = ItemType.Resource,
                    DisplayName = "Cords",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#2B1A23"),
                        ColorTranslator.FromHtml("#33232B"),
                        ColorTranslator.FromHtml("#2F1F27"),
                        ColorTranslator.FromHtml("#554444"),
                        ColorTranslator.FromHtml("#302028"),
                        ColorTranslator.FromHtml("#443344"),
                        ColorTranslator.FromHtml("#27161F"),
                        ColorTranslator.FromHtml("#502F27")
                    },
                    // Đặc trưng của Dây Da (Cords): Vùng bóng tối giữa các nếp gấp cuộn dây da có ánh tím/nâu tía (wineShadow: R > G, B - G >= 5, Total <= 160 đạt >= 400 px, thực tế > 1000 px),
                    // thân da màu sô-cô-la tối (chocoBody: R[35..90], G[20..55], B[25..55], R - G >= 12 đạt >= 800 px, thực tế > 2000 px),
                    // không có điểm sáng vàng chói (goldHighlight < 100 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int wineShadow = pixels.Count(p => (p.R > p.G) && (p.B - p.G >= 5) && (p.R + p.G + p.B <= 160));
                        int chocoBody = pixels.Count(p => p.R >= 35 && p.R <= 90 && p.G >= 20 && p.G <= 55 && p.B >= 25 && p.B <= 55 && (p.R - p.G >= 12));
                        int goldHighlight = pixels.Count(p => p.R >= 130 && p.G >= 95 && p.B >= 55 && (p.R - p.G >= 20) && (p.G - p.B >= 20) && (p.R + p.G + p.B >= 300));
                        int flkDeepOxblood = pixels.Count(p => p.R >= 44 && p.R <= 78 && p.G >= 14 && p.G <= 28 && p.B >= 14 && p.B <= 28 && (p.R - p.G >= 27) && Math.Abs(p.G - p.B) <= 5);

                        if (wineShadow >= 400 && chocoBody >= 800 && goldHighlight < 100 && flkDeepOxblood < 300)
                        {
                            return 10000.0 + (wineShadow * 3.0) + chocoBody;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Leather,
                    Type = ItemType.Resource,
                    DisplayName = "Leather",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#482716"),
                        ColorTranslator.FromHtml("#543323"),
                        ColorTranslator.FromHtml("#502F1F"),
                        ColorTranslator.FromHtml("#523121"),
                        ColorTranslator.FromHtml("#583727"),
                        ColorTranslator.FromHtml("#401F16"),
                        ColorTranslator.FromHtml("#4F2D1C"),
                        ColorTranslator.FromHtml("#2E150D")
                    },
                    // Đặc trưng của Da Thuộc (Leather): Thân tấm da màu nâu da bò (saddleBrown: R[60..100], G[30..65], B[15..45], R-G >= 25, G-B >= 10 đạt >= 700 px, thực tế 1600-1750 px),
                    // nếp gấp cuộn da màu nâu tối (leatherCrease: R[40..65], G[18..35], B[10..25], R-G >= 20, G-B >= 5 đạt >= 300 px, thực tế > 800 px).
                    // Không có càng khóa thép (steelShackle <= 50 px, khác Lock), không có bóng tím/rượu (wineShadow < 250 px, khác Cords).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int saddleBrown = pixels.Count(p => p.R >= 60 && p.R <= 100 && p.G >= 30 && p.G <= 65 && p.B >= 15 && p.B <= 45 && (p.R - p.G >= 25) && (p.G - p.B >= 10));
                        int leatherCrease = pixels.Count(p => p.R >= 40 && p.R <= 65 && p.G >= 18 && p.G <= 35 && p.B >= 10 && p.B <= 25 && (p.R - p.G >= 20) && (p.G - p.B >= 5));
                        int steelShackle = pixels.Count(p => p.R >= 60 && p.R <= 150 && p.G >= 60 && p.G <= 150 && p.B >= 60 && p.B <= 155 && Math.Abs(p.R - p.G) <= 12 && Math.Abs(p.G - p.B) <= 12 && (p.R + p.G + p.B >= 200));
                        int wineShadow = pixels.Count(p => (p.R > p.G) && (p.B - p.G >= 5) && (p.R + p.G + p.B <= 160));

                        if (saddleBrown >= 700 && leatherCrease >= 300 && steelShackle <= 50 && wineShadow < 250)
                        {
                            return 10000.0 + (saddleBrown * 2.0) + (leatherCrease * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Cloth,
                    Type = ItemType.Resource,
                    DisplayName = "Cloth",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A9774E"),
                        ColorTranslator.FromHtml("#97664D"),
                        ColorTranslator.FromHtml("#9C6B52"),
                        ColorTranslator.FromHtml("#A67453"),
                        ColorTranslator.FromHtml("#AE7C5B"),
                        ColorTranslator.FromHtml("#B38258"),
                        ColorTranslator.FromHtml("#A06F4E"),
                        ColorTranslator.FromHtml("#A57352")
                    },
                    // Đặc trưng của Vải (Cloth): Mặt vải dệt màu be nâu đào đặc trưng (clothPeach: R[120..185], R-G >= 42, G-B[15..33] đạt >= 500 px, thực tế > 1580 px),
                    // dải sợi dệt sáng màu (clothLinen: R[145..185], G[95..135], B[65..95], R-G[40..58], G-B[20..36] đạt >= 350 px, thực tế 866-902 px).
                    // Loại trừ ThickCloth (có >= 230 px tcTaupe sáng kem nhạt trong khi Cloth chỉ <= 12 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int oakAmber = pixels.Count(p => p.R >= 125 && p.R <= 180 && p.G >= 75 && p.G <= 130 && p.B >= 25 && p.B <= 80 && (p.R - p.G >= 45) && (p.G - p.B >= 45));
                        int oakDarkGold = pixels.Count(p => p.R >= 110 && p.R <= 160 && p.G >= 60 && p.G <= 108 && p.B >= 22 && p.B <= 60 && (p.R - p.G >= 45) && (p.G - p.B >= 30) && (p.R + p.G + p.B <= 320));
                        if (oakAmber >= 120 || oakDarkGold >= 200) return 0.0;

                        int tcTaupe = pixels.Count(p => p.R >= 175 && p.R <= 198 && p.G >= 134 && p.B >= 102 && (p.R - p.G >= 35) && (p.R - p.G <= 44) && (p.G - p.B >= 28) && (p.G - p.B <= 38));
                        if (tcTaupe >= 60) return 0.0;

                        int clothLinen = pixels.Count(p => p.R >= 145 && p.R <= 185 && p.G >= 95 && p.G <= 135 && p.B >= 65 && p.B <= 95 && (p.R - p.G >= 40) && (p.R - p.G <= 58) && (p.G - p.B >= 20) && (p.G - p.B <= 36));
                        int clothPeach = pixels.Count(p => p.R >= 120 && p.R <= 185 && (p.R - p.G >= 42) && (p.G - p.B >= 15) && (p.G - p.B <= 33));

                        if (clothPeach >= 500 && clothLinen >= 350)
                        {
                            return 10000.0 + (clothLinen * 2.0) + clothPeach;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.ThickCloth,
                    Type = ItemType.Resource,
                    DisplayName = "Thick Cloth",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#CB9968"),
                        ColorTranslator.FromHtml("#B78E6D"),
                        ColorTranslator.FromHtml("#B48B6A"),
                        ColorTranslator.FromHtml("#CC9A69"),
                        ColorTranslator.FromHtml("#BB9271"),
                        ColorTranslator.FromHtml("#B38A69"),
                        ColorTranslator.FromHtml("#BF9675"),
                        ColorTranslator.FromHtml("#B6845B")
                    },
                    // Đặc trưng của Vải dày (Thick Cloth): Các nếp vải kem nâu sáng đặc trưng (tcTaupe >= 80 px, thực tế 230-264 px; tcCream >= 220 px, thực tế 500-550 px)
                    // kết hợp nền vải nâu đào (clothPeach >= 500, clothLinen >= 350). Hoàn toàn tách biệt với Cloth thường (tcTaupe chỉ 4-12 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int oakAmber = pixels.Count(p => p.R >= 125 && p.R <= 180 && p.G >= 75 && p.G <= 130 && p.B >= 25 && p.B <= 80 && (p.R - p.G >= 45) && (p.G - p.B >= 45));
                        int oakDarkGold = pixels.Count(p => p.R >= 110 && p.R <= 160 && p.G >= 60 && p.G <= 108 && p.B >= 22 && p.B <= 60 && (p.R - p.G >= 45) && (p.G - p.B >= 30) && (p.R + p.G + p.B <= 320));
                        if (oakAmber >= 80 || oakDarkGold >= 150) return 0.0;

                        int tcTaupe = pixels.Count(p => p.R >= 175 && p.R <= 198 && p.G >= 134 && p.B >= 102 && (p.R - p.G >= 35) && (p.R - p.G <= 44) && (p.G - p.B >= 28) && (p.G - p.B <= 38));
                        int tcCream = pixels.Count(p => p.R >= 178 && p.G >= 138 && p.B >= 104 && (p.R - p.G >= 35) && (p.G - p.B >= 28) && (p.R + p.G + p.B >= 420));
                        int clothLinen = pixels.Count(p => p.R >= 145 && p.R <= 185 && p.G >= 95 && p.G <= 135 && p.B >= 65 && p.B <= 95 && (p.R - p.G >= 40) && (p.R - p.G <= 58) && (p.G - p.B >= 20) && (p.G - p.B <= 36));
                        int clothPeach = pixels.Count(p => p.R >= 120 && p.R <= 185 && (p.R - p.G >= 42) && (p.G - p.B >= 15) && (p.G - p.B <= 33));

                        if (tcTaupe >= 80 && tcCream >= 220 && clothPeach >= 500 && clothLinen >= 350)
                        {
                            return 10000.0 + (tcTaupe * 5.0) + (tcCream * 2.0) + clothLinen;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Rope,
                    Type = ItemType.Resource,
                    DisplayName = "Rope",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#907756"),
                        ColorTranslator.FromHtml("#866D54"),
                        ColorTranslator.FromHtml("#856C53"),
                        ColorTranslator.FromHtml("#8E6D54"),
                        ColorTranslator.FromHtml("#7B6249"),
                        ColorTranslator.FromHtml("#634A3A"),
                        ColorTranslator.FromHtml("#624939"),
                        ColorTranslator.FromHtml("#614837")
                    },
                    // Đặc trưng của Dây thừng (Rope): Sợi thừng xoắn màu be xám nâu xỉn đặc trưng (ropeMid: R[115..155], G[90..130], B[68..98], R-G[20..29] >= 200 px, thực tế 622-692 px)
                    // và nếp tối xoắn (ropeDark: R[85..115], G[60..88], B[45..72], R-G[20..28] >= 120 px, thực tế 392-494 px).
                    // Hoàn toàn tách biệt với Fibula/GoldRing/Cloth/Bowstring (các item khác có ropeMid <= 69, ropeDark <= 80).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int purpleGrate = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleGrate >= 100) return 0.0;

                        int ropeMid = pixels.Count(p => p.R >= 115 && p.R <= 155 && p.G >= 90 && p.G <= 130 && p.B >= 68 && p.B <= 98 && (p.R - p.G >= 20) && (p.R - p.G <= 29) && (p.G - p.B >= 20) && (p.G - p.B <= 36));
                        int ropeDark = pixels.Count(p => p.R >= 85 && p.R <= 115 && p.G >= 60 && p.G <= 88 && p.B >= 45 && p.B <= 72 && (p.R - p.G >= 20) && (p.R - p.G <= 28) && (p.G - p.B >= 12) && (p.G - p.B <= 22));

                        if (ropeMid >= 200 && ropeDark >= 120)
                        {
                            return 10000.0 + (ropeMid * 3.0) + (ropeDark * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.HoneyBarrel,
                    Type = ItemType.Consumable,
                    DisplayName = "Honey Barrel",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#714837"),
                        ColorTranslator.FromHtml("#61372F"),
                        ColorTranslator.FromHtml("#824F3E"),
                        ColorTranslator.FromHtml("#52313A"),
                        ColorTranslator.FromHtml("#502F37"),
                        ColorTranslator.FromHtml("#4C2B33"),
                        ColorTranslator.FromHtml("#A97F56"),
                        ColorTranslator.FromHtml("#A37950")
                    },
                    // Đặc trưng của Thùng mật ong (Honey Barrel): Thân thùng gỗ hai sắc thái rõ rệt — bóng gỗ đỏ tía bên trái (hbMahogany: R[65..92], G[35..58], B[42..66], B-G >= 3 đạt >= 90 px, thực tế 409-484 px),
                    // thân gỗ nâu tuyết tùng (hbCedar >= 220 px, thực tế 830-1010 px) và gỗ sáng bên phải (hbWarmWood >= 180 px, thực tế 646-782 px).
                    // Không dùng ngưỡng purpleGrate < 100 vì bóng gỗ đỏ tía (#52313A) có B-G=9 và R+G+B=189.
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int hbMahogany = pixels.Count(p => p.R >= 65 && p.R <= 92 && p.G >= 35 && p.G <= 58 && p.B >= 42 && p.B <= 66 && (p.R - p.G >= 25) && (p.B - p.G >= 3));
                        int hbCedar = pixels.Count(p => p.R >= 88 && p.R <= 145 && p.G >= 48 && p.G <= 92 && p.B >= 38 && p.B <= 75 && (p.R - p.G >= 35) && (p.G - p.B >= 4) && (p.G - p.B <= 22));
                        int hbWarmWood = pixels.Count(p => p.R >= 145 && p.R <= 190 && p.G >= 100 && p.G <= 145 && p.B >= 65 && p.B <= 105 && (p.R - p.G >= 34) && (p.G - p.B >= 28));

                        if (hbMahogany >= 90 && hbCedar >= 220 && hbWarmWood >= 180)
                        {
                            return 10000.0 + (hbMahogany * 4.0) + (hbWarmWood * 2.0) + hbCedar;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Mirror,
                    Type = ItemType.Special,
                    DisplayName = "Mirror",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#BABABA"),
                        ColorTranslator.FromHtml("#BBBBBB"),
                        ColorTranslator.FromHtml("#B8B8B8"),
                        ColorTranslator.FromHtml("#BCBCBC"),
                        ColorTranslator.FromHtml("#7F6666"),
                        ColorTranslator.FromHtml("#A08877"),
                        ColorTranslator.FromHtml("#A38A79"),
                        ColorTranslator.FromHtml("#867E8E")
                    },
                    // Đặc trưng của Gương (Mirror): Viền bạc sáng trung tính rất cao (mirrorSilver: R[165..225], |R-G| <= 5, |G-B| <= 5 đạt >= 150 px, thực tế 493-603 px,
                    // các vật phẩm khác <= 33 px) kết hợp mặt gương nâu hồng/be nhạt (mirrorGlass >= 250 px, thực tế 707-738 px; mirrorMauve >= 100 px, thực tế 330-363 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int mirrorSilver = pixels.Count(p => p.R >= 165 && p.R <= 225 && Math.Abs(p.R - p.G) <= 5 && Math.Abs(p.G - p.B) <= 5);
                        int mirrorGlass = pixels.Count(p => p.R >= 120 && p.R <= 175 && p.G >= 95 && p.G <= 148 && p.B >= 95 && p.B <= 135 && (p.R - p.G >= 18) && (p.G - p.B >= 0) && (p.G - p.B <= 22));
                        int mirrorMauve = pixels.Count(p => p.R >= 115 && p.R <= 145 && p.G >= 95 && p.G <= 118 && p.B >= 95 && p.B <= 118 && (p.R - p.G >= 18) && Math.Abs(p.G - p.B) <= 5);

                        if (mirrorSilver >= 150 && mirrorGlass >= 250 && mirrorMauve >= 100)
                        {
                            return 10000.0 + (mirrorSilver * 4.0) + (mirrorMauve * 2.0) + mirrorGlass;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Silverware,
                    DisplayName = "Silverware",
                    Type = ItemType.Special,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#848484"),
                        ColorTranslator.FromHtml("#939393")
                    },
                    CustomEvaluator = (pixels) =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int purpleGrate = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        int mirrorGlass = pixels.Count(p => p.R >= 120 && p.R <= 175 && p.G >= 95 && p.G <= 148 && p.B >= 95 && p.B <= 135 && (p.R - p.G >= 18) && (p.G - p.B >= 0) && (p.G - p.B <= 22));
                        int lockBrownBody = pixels.Count(p => p.R >= 65 && p.R <= 120 && p.G >= 35 && p.G <= 75 && p.B >= 20 && p.B <= 55 && (p.R - p.G >= 20) && (p.G - p.B >= 10));

                        if (purpleGrate >= 30 || mirrorGlass >= 30 || lockBrownBody >= 50) return 0.0;

                        int silvCool = pixels.Count(p => p.R >= 120 && p.R <= 235 && p.G >= 120 && p.G <= 235 && p.B >= 120 && p.B <= 240 && Math.Abs(p.R - p.G) <= 6 && (p.B - p.G >= -2) && (p.B - p.G <= 18));
                        int silvMid = pixels.Count(p => p.R >= 125 && p.R <= 165 && p.G >= 125 && p.G <= 165 && p.B >= 125 && p.B <= 175 && Math.Abs(p.R - p.G) <= 5 && (p.B - p.G >= 0) && (p.B - p.G <= 15));

                        if (silvCool >= 110 && silvMid >= 55)
                        {
                            return 10000.0 + (silvCool * 4.0) + (silvMid * 3.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Bandage,
                    Type = ItemType.Consumable,
                    DisplayName = "Bandage",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#897878"),
                        ColorTranslator.FromHtml("#8A7979"),
                        ColorTranslator.FromHtml("#877676"),
                        ColorTranslator.FromHtml("#B79E8E"),
                        ColorTranslator.FromHtml("#A88F87"),
                        ColorTranslator.FromHtml("#8C7B8C")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int mirrorSilver = pixels.Count(p => p.R >= 165 && p.R <= 225 && Math.Abs(p.R - p.G) <= 5 && Math.Abs(p.G - p.B) <= 5);
                        if (mirrorSilver >= 80) return 0.0;

                        int bdMauve = pixels.Count(p => p.R >= 115 && p.R <= 155 && p.G >= 98 && p.G <= 135 && p.B >= 115 && p.B <= 155 && (p.R - p.G >= 13) && (p.B - p.G >= 10) && Math.Abs(p.R - p.B) <= 8);
                        int bdCream = pixels.Count(p => p.R >= 158 && p.R <= 195 && p.G >= 132 && p.G <= 168 && p.B >= 122 && p.B <= 155 && (p.R - p.G >= 18) && (p.R - p.G <= 30) && (p.G - p.B >= 4) && (p.G - p.B <= 20));
                        int bdTaupe = pixels.Count(p => p.R >= 120 && p.R <= 155 && p.G >= 103 && p.G <= 138 && p.B >= 103 && p.B <= 145 && (p.R - p.G >= 13) && (p.R - p.G <= 23) && Math.Abs(p.G - p.B) <= 5);

                        if (bdMauve >= 120 && bdCream >= 140 && bdTaupe >= 140)
                        {
                            return 10000.0 + (bdMauve * 4.0) + (bdCream * 2.0) + bdTaupe;
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.LeekSeed,
                    Type = ItemType.Resource,
                    DisplayName = "Leek Seed",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#211921"),
                        ColorTranslator.FromHtml("#241C24"),
                        ColorTranslator.FromHtml("#251D25"),
                        ColorTranslator.FromHtml("#33333B"),
                        ColorTranslator.FromHtml("#2A2A32"),
                        ColorTranslator.FromHtml("#282028")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int purpleCount = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleCount >= 200) return 0.0;

                        int lsCoolRim = pixels.Count(p => p.R >= 36 && p.R <= 62 && Math.Abs(p.R - p.G) <= 2 && (p.B - p.G >= 5) && (p.B - p.G <= 12));
                        int lsDarkPurp = pixels.Count(p => p.R >= 26 && p.R <= 70 && p.G >= 18 && p.G <= 62 && p.B >= 26 && p.B <= 70 && (p.R - p.G >= 6) && (p.B - p.G >= 6) && Math.Abs(p.R - p.B) <= 4 && (p.R + p.G + p.B <= 195));

                        if (lsCoolRim >= 180 && lsDarkPurp >= 180)
                        {
                            return 10000.0 + (lsCoolRim * 3.0) + (lsDarkPurp * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.OakSeed,
                    Type = ItemType.Resource,
                    DisplayName = "Oak Seed",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#8E5B28"),
                        ColorTranslator.FromHtml("#8D5A27"),
                        ColorTranslator.FromHtml("#885522"),
                        ColorTranslator.FromHtml("#915E2B"),
                        ColorTranslator.FromHtml("#A76D44"),
                        ColorTranslator.FromHtml("#AA7744")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int purpleCount = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        if (purpleCount >= 50) return 0.0;

                        int oakAmber = pixels.Count(p => p.R >= 125 && p.R <= 180 && p.G >= 75 && p.G <= 130 && p.B >= 25 && p.B <= 80 && (p.R - p.G >= 45) && (p.G - p.B >= 45));
                        int oakDarkGold = pixels.Count(p => p.R >= 110 && p.R <= 160 && p.G >= 60 && p.G <= 108 && p.B >= 22 && p.B <= 60 && (p.R - p.G >= 45) && (p.G - p.B >= 30) && (p.R + p.G + p.B <= 320));

                        if (oakAmber >= 135 && oakDarkGold >= 280)
                        {
                            return 10000.0 + (oakAmber * 4.0) + (oakDarkGold * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.PumpkinSeeds,
                    Type = ItemType.Resource,
                    DisplayName = "Pumpkin Seeds",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#E0D8BF"),
                        ColorTranslator.FromHtml("#E0BF96"),
                        ColorTranslator.FromHtml("#D8B78E"),
                        ColorTranslator.FromHtml("#DCBB92"),
                        ColorTranslator.FromHtml("#EAC18F"),
                        ColorTranslator.FromHtml("#713E2D")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int oakAmber = pixels.Count(p => p.R >= 125 && p.R <= 180 && p.G >= 75 && p.G <= 130 && p.B >= 25 && p.B <= 80 && (p.R - p.G >= 45) && (p.G - p.B >= 45));
                        if (oakAmber >= 80) return 0.0;

                        int combBright = pixels.Count(p => p.R >= 230 && p.G >= 205 && p.B >= 170 && (p.R + p.G + p.B >= 610) && (p.R - p.G >= 10) && (p.G - p.B >= 12));
                        if (combBright >= 350) return 0.0;

                        int pmpCream = pixels.Count(p => p.R >= 205 && p.G >= 172 && p.B >= 130 && (p.R - p.G >= 15) && (p.R - p.G <= 45) && (p.G - p.B >= 20) && (p.G - p.B <= 52));
                        int pmpIvory = pixels.Count(p => p.R >= 210 && p.G >= 195 && p.B >= 160 && (p.R - p.G >= 4) && (p.G - p.B >= 18) && (p.R + p.G + p.B >= 565));
                        int pmpShadow = pixels.Count(p => p.R >= 95 && p.R <= 135 && p.G >= 50 && p.G <= 82 && p.B >= 35 && p.B <= 65 && (p.R - p.G >= 38) && (p.G - p.B >= 10));

                        if (pmpCream >= 450 && pmpIvory >= 250 && pmpShadow >= 65)
                        {
                            return 10000.0 + (pmpCream * 3.0) + (pmpIvory * 4.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Comb,
                    Type = ItemType.Special,
                    DisplayName = "Comb",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#F5E5D4"),
                        ColorTranslator.FromHtml("#F0D7B6"),
                        ColorTranslator.FromHtml("#EDD4B3"),
                        ColorTranslator.FromHtml("#EEDDBB"),
                        ColorTranslator.FromHtml("#7D5B39"),
                        ColorTranslator.FromHtml("#DDAA88")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int combBright = pixels.Count(p => p.R >= 230 && p.G >= 205 && p.B >= 170 && (p.R + p.G + p.B >= 610) && (p.R - p.G >= 10) && (p.G - p.B >= 12));
                        int combShadow = pixels.Count(p => p.R >= 115 && p.R <= 145 && p.G >= 82 && p.G <= 112 && p.B >= 48 && p.B <= 78 && (p.R - p.G >= 30) && (p.R - p.G <= 40) && (p.G - p.B >= 28) && (p.G - p.B <= 40));
                        int pmpShadow = pixels.Count(p => p.R >= 95 && p.R <= 135 && p.G >= 50 && p.G <= 82 && p.B >= 35 && p.B <= 65 && (p.R - p.G >= 38) && (p.G - p.B >= 10));

                        if (combBright >= 400 && combShadow >= 75 && pmpShadow < 60)
                        {
                            return 10000.0 + (combBright * 4.0) + (combShadow * 3.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Candle,
                    Type = ItemType.Resource,
                    DisplayName = "Candle",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#D9B06E"),
                        ColorTranslator.FromHtml("#B69460"),
                        ColorTranslator.FromHtml("#AD8452"),
                        ColorTranslator.FromHtml("#A67C4B"),
                        ColorTranslator.FromHtml("#8F663C"),
                        ColorTranslator.FromHtml("#704A2E")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int cndHighlight = pixels.Count(p => p.R >= 165 && p.R <= 230 && p.G >= 125 && p.G <= 190 && p.B >= 70 && p.B <= 125 && (p.R - p.G >= 28) && (p.R - p.G <= 52) && (p.G - p.B >= 48));
                        int cndWaxGB = pixels.Count(p => p.R >= 135 && p.R <= 195 && p.G >= 95 && p.G <= 155 && p.B >= 55 && p.B <= 105 && (p.R - p.G >= 30) && (p.R - p.G <= 46) && (p.G - p.B >= p.R - p.G));

                        if (cndHighlight >= 360 && cndWaxGB >= 260)
                        {
                            return 10000.0 + (cndHighlight * 4.0) + (cndWaxGB * 3.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Skull,
                    Type = ItemType.Special,
                    DisplayName = "Skull",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#CBC2B2"),
                        ColorTranslator.FromHtml("#CCBBA3"),
                        ColorTranslator.FromHtml("#E0D8C8"),
                        ColorTranslator.FromHtml("#EDE5DC"),
                        ColorTranslator.FromHtml("#BBAB92"),
                        ColorTranslator.FromHtml("#463524"),
                        ColorTranslator.FromHtml("#5D4C3B")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int skBonePale = pixels.Count(p => p.R >= 185 && p.G >= 175 && p.B >= 155 && (p.R - p.G >= 4) && (p.R - p.G <= 16) && (p.G - p.B >= 7) && (p.G - p.B <= 26) && (p.R + p.G + p.B >= 520));
                        int skBrightBone = pixels.Count(p => p.R >= 200 && p.G >= 190 && p.B >= 172 && (p.R - p.G >= 4) && (p.R - p.G <= 13) && (p.G - p.B >= 7) && (p.G - p.B <= 20));
                        int skDarkUmber = pixels.Count(p => p.R >= 55 && p.R <= 98 && p.G >= 40 && p.G <= 80 && p.B >= 26 && p.B <= 64 && (p.R - p.G >= 12) && (p.R - p.G <= 22) && (p.G - p.B >= 12) && (p.G - p.B <= 22));

                        if (skBonePale >= 300 && skBrightBone >= 180 && skDarkUmber >= 180)
                        {
                            return 10000.0 + (skBonePale * 3.0) + (skDarkUmber * 4.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.FlaxFibber,
                    Type = ItemType.Resource,
                    DisplayName = "Flax Fiber",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#664433"),
                        ColorTranslator.FromHtml("#6D4C3B"),
                        ColorTranslator.FromHtml("#694837"),
                        ColorTranslator.FromHtml("#9D7C5B"),
                        ColorTranslator.FromHtml("#A1805F"),
                        ColorTranslator.FromHtml("#AA8866")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int flxChestnut = pixels.Count(p => p.R >= 92 && p.R <= 118 && p.G >= 60 && p.G <= 84 && p.B >= 44 && p.B <= 66 && (p.R - p.G >= 28) && (p.R - p.G <= 38) && (p.G - p.B >= 13) && (p.G - p.B <= 21));
                        int flxStrand = pixels.Count(p => p.R >= 145 && p.R <= 182 && p.G >= 112 && p.G <= 148 && p.B >= 78 && p.B <= 114 && (p.R - p.G >= 28) && (p.R - p.G <= 38) && (p.G - p.B >= 28) && (p.G - p.B <= 38));
                        int fibSpec = pixels.Count(p => p.R >= 185 && p.G >= 150 && p.B >= 95);

                        if (flxChestnut >= 140 && flxStrand >= 160 && fibSpec < 50)
                        {
                            return 10000.0 + (flxStrand * 4.0) + (flxChestnut * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Hourglass,
                    Type = ItemType.Special,
                    DisplayName = "Hourglass",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A3482F"),
                        ColorTranslator.FromHtml("#B3502F"),
                        ColorTranslator.FromHtml("#B07D4A"),
                        ColorTranslator.FromHtml("#602D1C"),
                        ColorTranslator.FromHtml("#4C4C5D"),
                        ColorTranslator.FromHtml("#444455"),
                        ColorTranslator.FromHtml("#7799AA")
                    },
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int hgWoodRed = pixels.Count(p => p.R >= 120 && p.R <= 198 && p.G >= 48 && p.G <= 95 && p.B >= 28 && p.B <= 60 && (p.R - p.G >= 60));
                        int hgGlassBlue = pixels.Count(p => p.R >= 55 && p.R <= 92 && p.G >= 55 && p.G <= 92 && p.B >= 72 && p.B <= 112 && Math.Abs(p.R - p.G) <= 5 && (p.B - p.G >= 12) && (p.B - p.G <= 25));

                        if (hgWoodRed >= 120 && hgGlassBlue >= 40)
                        {
                            return 10000.0 + (hgWoodRed * 4.0) + (hgGlassBlue * 4.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.PineLog,
                    Type = ItemType.Resource,
                    DisplayName = "Pine Log",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#3B231A"),
                        ColorTranslator.FromHtml("#442B23"),
                        ColorTranslator.FromHtml("#472E26"),
                        ColorTranslator.FromHtml("#4A3129"),
                        ColorTranslator.FromHtml("#4C332B"),
                        ColorTranslator.FromHtml("#8E6C5A")
                    },
                    // Đặc trưng của Gỗ Thông (Pine Log): Vỏ gỗ nâu gụ sẫm (pineBark: R[52..86], G[28..60], B[18..52], R-G[19..29], G-B[5..14] >= 750 px, thực tế 1852-2114 px),
                    // kết hợp với mặt cắt lõi gỗ vân tròn màu hồng be nhạt (pineRing: R[130..195], G[95..155], B[80..135], R-G[26..44], G-B[10..26] >= 150 px, thực tế 382-495 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int pineBark = pixels.Count(p => p.R >= 52 && p.R <= 86 && p.G >= 28 && p.G <= 60 && p.B >= 18 && p.B <= 52 && (p.R - p.G >= 19) && (p.R - p.G <= 29) && (p.G - p.B >= 5) && (p.G - p.B <= 14));
                        int pineRing = pixels.Count(p => p.R >= 130 && p.R <= 195 && p.G >= 95 && p.G <= 155 && p.B >= 80 && p.B <= 135 && (p.R - p.G >= 26) && (p.R - p.G <= 44) && (p.G - p.B >= 10) && (p.G - p.B <= 26));

                        if (pineBark >= 750 && pineRing >= 150)
                        {
                            return 10000.0 + (pineBark * 2.0) + (pineRing * 4.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.EmptyFlask,
                    Type = ItemType.Resource,
                    DisplayName = "Empty Flask",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#371616"),
                        ColorTranslator.FromHtml("#3B1A1A"),
                        ColorTranslator.FromHtml("#5C2B23"),
                        ColorTranslator.FromHtml("#96543B"),
                        ColorTranslator.FromHtml("#692F27"),
                        ColorTranslator.FromHtml("#783E2E")
                    },
                    // Đặc trưng của Bình Da Rỗng (Empty Flask): Vùng bóng tối đỏ mận sâu (flkDeepOxblood: R[44..78], G[14..28], B[14..28], R-G >= 27, |G-B| <= 5 >= 350 px, thực tế 842-1065 px),
                    // kết hợp với phần thân da nâu đỏ khâu chỉ (flkMid: R[84..165], G[38..96], B[28..70], R-G >= 36, G-B[4..30] >= 600 px, thực tế 1469-1820 px) và loại trừ Ale (aleMugDark < 450 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int flkDeepOxblood = pixels.Count(p => p.R >= 44 && p.R <= 78 && p.G >= 14 && p.G <= 28 && p.B >= 14 && p.B <= 28 && (p.R - p.G >= 27) && Math.Abs(p.G - p.B) <= 5);
                        int flkMid = pixels.Count(p => p.R >= 84 && p.R <= 165 && p.G >= 38 && p.G <= 96 && p.B >= 28 && p.B <= 70 && (p.R - p.G >= 36) && (p.G - p.B >= 4) && (p.G - p.B <= 30));
                        int aleMugDark = pixels.Count(p => p.R >= 55 && p.R <= 96 && p.G >= 29 && p.G <= 55 && p.B >= 29 && p.B <= 55 && Math.Abs(p.G - p.B) <= 3 && (p.R - p.G >= 26) && (p.R - p.G <= 45));

                        if (flkDeepOxblood >= 350 && flkMid >= 600 && aleMugDark < 450)
                        {
                            return 10000.0 + (flkDeepOxblood * 4.0) + (flkMid * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.RawMeat,
                    Type = ItemType.Consumable,
                    DisplayName = "Raw Meat",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#79272F"),
                        ColorTranslator.FromHtml("#71272F"),
                        ColorTranslator.FromHtml("#7D2B33"),
                        ColorTranslator.FromHtml("#993344"),
                        ColorTranslator.FromHtml("#C14A5B"),
                        ColorTranslator.FromHtml("#6D232B")
                    },
                    // Đặc trưng của Thịt Sống (Raw Meat): Sắc đỏ hồng thịt tươi có B > G (meatRuby: R[100..220], G[25..105], B[32..118], R-G >= 68, R-B >= 55, B-G >= 4 >= 1200 px, thực tế 3485-3783 px),
                    // kết hợp với các đường vân mỡ hồng nhạt (meatFat: R[180..238], G[125..200], B[135..210], R-G >= 22, B-G >= 4 >= 100 px, thực tế 267-336 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int meatRuby = pixels.Count(p => p.R >= 100 && p.R <= 220 && p.G >= 25 && p.G <= 105 && p.B >= 32 && p.B <= 118 && (p.R - p.G >= 68) && (p.R - p.B >= 55) && (p.B - p.G >= 4));
                        int meatFat = pixels.Count(p => p.R >= 180 && p.R <= 238 && p.G >= 125 && p.G <= 200 && p.B >= 135 && p.B <= 210 && (p.R - p.G >= 22) && (p.B - p.G >= 4));

                        if (meatRuby >= 1200 && meatFat >= 100)
                        {
                            return 10000.0 + (meatRuby * 2.0) + (meatFat * 4.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.Ale,
                    Type = ItemType.Consumable,
                    DisplayName = "Ale",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#542B2B"),
                        ColorTranslator.FromHtml("#401F1F"),
                        ColorTranslator.FromHtml("#582F2F"),
                        ColorTranslator.FromHtml("#552C2C"),
                        ColorTranslator.FromHtml("#593030"),
                        ColorTranslator.FromHtml("#78483E")
                    },
                    // Đặc trưng của Vại Bia (Ale): Thân vại gỗ nâu mận cân kênh G~B (aleMugDark: R[55..96], G[29..55], B[29..55], |G-B| <= 3, R-G[26..45] >= 650 px, thực tế 1433-2334 px),
                    // kết hợp phần gỗ sáng ấm (flkMid >= 450 px, thực tế 1065-1541 px) và không có bóng đỏ mận cực đậm của Empty Flask (flkDeepOxblood < 300 px).
                    CustomEvaluator = pixels =>
                    {
                        if (pixels.Count < 100) return 0.0;

                        int aleMugDark = pixels.Count(p => p.R >= 55 && p.R <= 96 && p.G >= 29 && p.G <= 55 && p.B >= 29 && p.B <= 55 && Math.Abs(p.G - p.B) <= 3 && (p.R - p.G >= 26) && (p.R - p.G <= 45));
                        int flkMid = pixels.Count(p => p.R >= 84 && p.R <= 165 && p.G >= 38 && p.G <= 96 && p.B >= 28 && p.B <= 70 && (p.R - p.G >= 36) && (p.G - p.B >= 4) && (p.G - p.B <= 30));
                        int flkDeepOxblood = pixels.Count(p => p.R >= 44 && p.R <= 78 && p.G >= 14 && p.G <= 28 && p.B >= 14 && p.B <= 28 && (p.R - p.G >= 27) && Math.Abs(p.G - p.B) <= 5);

                        if (aleMugDark >= 650 && flkMid >= 450 && flkDeepOxblood < 300)
                        {
                            return 10000.0 + (aleMugDark * 2.0) + (flkMid * 2.0);
                        }

                        return 0.0;
                    }
                },
                new Item
                {
                    Name = ItemName.SoulShard,
                    Type = ItemType.Resource,
                    DisplayName = "Soul Shard",
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#646464"),
                        ColorTranslator.FromHtml("#777777"),
                        ColorTranslator.FromHtml("#505050")
                    },
                    CustomEvaluator = pixels =>
                    {
                        int purpleCount = pixels.Count(p => (p.B - p.G >= 7) && (p.R - p.G >= 7) && (p.R + p.G + p.B >= 180));
                        int tanRope = pixels.Count(p => p.R > p.G && p.G > p.B && (p.R - p.G >= 10) && (p.G - p.B >= 10));
                        int chainOliveRust = pixels.Count(p => (p.G - p.B >= 4));
                        int wireDarkBundle = pixels.Count(p => (p.R + p.G + p.B <= 50) && (p.G - p.B <= 3) && (p.R >= p.G));
                        if (purpleCount >= 20 || tanRope >= 80 || chainOliveRust >= 150 || wireDarkBundle >= 500) return 0.0;

                        int soulMatches = pixels.Count(p => Math.Abs(p.R - 100) <= 12 && Math.Abs(p.G - 100) <= 12 && Math.Abs(p.B - 100) <= 12);
                        return soulMatches >= 150 ? (10000.0 + soulMatches) : 0.0;
                    }
                }
            };
        }
    }
}
