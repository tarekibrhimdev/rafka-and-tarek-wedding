using System.Text.Json;

namespace WeddingInvitation.Pages;

/// <summary>
/// Default English / Arabic strings for the public invitation (adjust copy here as needed).
/// </summary>
public static class InviteCopyDefaults
{
    public static Dictionary<string, Dictionary<string, string>> Build(string coupleLine, string? coupleLineAr = null)
    {
        static (string first, string second) SplitCouple(string line)
        {
            var parts = line.Split('&', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? (parts[0], parts[1]) : (line.Trim(), "");
        }

        var (enFirst, enSecond) = SplitCouple(coupleLine);
        var (arFirst, arSecond) = string.IsNullOrWhiteSpace(coupleLineAr)
            ? (enFirst, enSecond)
            : SplitCouple(coupleLineAr.Trim());
        var enCoupleFull = coupleLine.Trim();
        var arCoupleFull = string.IsNullOrWhiteSpace(coupleLineAr) ? enCoupleFull : coupleLineAr.Trim();

        return new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["cin_couple_given_1"] = enFirst,
                ["cin_couple_given_2"] = enSecond,
                ["cin_couple_joiner"] = "&",
                ["cin_couple_full"] = enCoupleFull,

                ["slide0_kicker"] = "Wedding invitation",
                ["slide0_tagline"] =
                    "Together with their families, they joyfully invite you to celebrate their marriage.",

                ["lux_celebration_label"] = "The celebration",
                ["lux_date_note"] = "Formal attire · Evening under the stars",
                ["lux_finale_kicker"] = "With love",
                ["lux_swipe_hint"] = "Swipe",

                ["slide1_saveTheDate"] = "Save the date",
                ["slide1_month"] = "June 2026",

                ["slide2_intro"] =
                    $"Together with their families, {coupleLine} joyfully invite you to celebrate their marriage.",
                ["slide2_when"] = "Saturday · 27 June 2026",

                ["slide2_ceremony"] = "Ceremony",
                ["slide2_ceremonyTime"] = "6:00 PM",
                ["slide2_ceremonyPlace"] = "Saint Elias Catholic Church · Kherbet Qanafar",

                ["slide2_reception"] = "Reception",
                ["slide2_receptionTime"] = "8:00 PM",
                ["slide2_receptionPlace"] = "Royal Venue · Kefraya",

                ["cin_detail_ceremony_kicker"] = "Ceremony",
                ["cin_detail_ceremony_time"] = "6:00 PM",
                ["cin_detail_ceremony_venue"] = "Saint Elias Catholic Church",
                ["cin_detail_ceremony_meta"] = "Kherbet Qanafar · Catholic parish",
                ["cin_detail_reception_kicker"] = "Reception",
                ["cin_detail_reception_time"] = "8:00 PM",
                ["cin_detail_reception_venue"] = "Royal Venue",
                ["cin_detail_reception_meta"] = "Kefraya, Lebanon",

                ["slide2_verse"] =
                    "“What therefore God hath joined together, let not man put asunder.” — Matthew 19:6",

                ["slide2_rsvp"] = "RSVP",
                ["slide2_rsvpPhones"] = "81 646 800 · 71 675 254",
                ["slide2_rsvpDeadline"] = "Kindly confirm by 14 June 2026",

                ["slide2_gift"] =
                    "Gift contributions — Wish account ID: 20919566-03 · Tel: 81 646 800",

                ["slide2_footer"] = "Online RSVP will appear here soon.",

                ["weekday_0"] = "M",
                ["weekday_1"] = "T",
                ["weekday_2"] = "W",
                ["weekday_3"] = "T",
                ["weekday_4"] = "F",
                ["weekday_5"] = "S",
                ["weekday_6"] = "S",

                ["cin_loader_mark"] = "Gathering light",

                ["cin_enter_kicker"] = "You're invited",
                ["cin_enter_title"] = "Our love story",
                ["cin_enter_cta"] = "Begin",
                ["cin_swipe_begin"] = "Swipe to begin",
                ["cin_brand_save_the_date"] = "Save the date",
                ["cin_story_eyebrow"] = "Our story",
                ["cin_story_lead"] =
                    "Every chapter led us here — quiet rooms, loud joy, and the ease of home in each other.",
                ["cin_timeline_2023_caption"] = "The first gaze",
                ["cin_timeline_2023_body"] = "A winter night when time seemed to stand still.",
                ["cin_timeline_2024_caption"] = "The best of times",
                ["cin_timeline_2024_body"] = "We were in a relationship, having the best time of our lives.",
                ["cin_timeline_2025_caption"] = "Engaged",
                ["cin_timeline_2025_body"] = "I proposed — and we got engaged.",
                ["cin_timeline_2026_caption"] = "The wedding",
                ["cin_timeline_2026_body"] = "2026 — our wedding day, and forever yes.",                ["cin_proposal_eyebrow"] = "",
                ["cin_proposal_quote"] =
                    "What God has joined together, let no one separate. Mark 10:9",
                ["cin_proposal_body"] = "",
                ["cin_label_date"] = "Date",
                ["cin_detail_date_value"] = "Saturday, June 27, 2026",
                ["cin_label_time"] = "Time",
                ["cin_label_venue"] = "Venue",
                ["cin_label_attire"] = "Attire",
                ["cin_countdown_eyebrow"] = "Counting the moments",
                ["cin_countdown_line1"] = "Until we say",
                ["cin_countdown_line2"] = "I do",
                ["cin_cd_days"] = "Days",
                ["cin_cd_hrs"] = "Hrs",
                ["cin_cd_min"] = "Min",
                ["cin_cd_sec"] = "Sec",
                ["cin_gallery_eyebrow"] = "A quiet archive",
                ["cin_gallery_lead"] = "Frames of warmth \u2014 not posed, only felt.",
                ["cin_location_eyebrow"] = "Find us",
                ["cin_location_lead"] = "Ceremony & reception — Beqaa Valley, Lebanon",
                ["cin_location_poetic"] = "An evening beneath the Bekaa sky.",
                ["cin_location_church_kicker"] = "Ceremony",
                ["cin_location_church_title"] = "Saint Elias Catholic Church",
                ["cin_location_church_meta"] = "Kherbet Qanafar · Lebanon",
                ["cin_location_venue_kicker"] = "Reception",
                ["cin_location_venue_title"] = "Royal Venue · Kefraya",
                ["cin_location_venue_meta"] = "Western Beqaa · Kefraya · Lebanon",
                ["cin_maps_open"] = "Open in Maps",
                ["cin_maps_open_aria"] = "Open this place in Google Maps",
                ["cin_rsvp_eyebrow"] = "Kindly reply",
                ["cin_rsvp_line1"] = "Will you celebrate",
                ["cin_rsvp_line2"] = "with us?",
                ["cin_rsvp_label_name"] = "Your name",
                ["cin_rsvp_ph_name"] = "Full name",
                ["cin_rsvp_label_attend"] = "Attending",
                ["cin_rsvp_yes"] = "Joyfully yes",
                ["cin_rsvp_no"] = "Regretfully no",
                ["cin_rsvp_label_guests"] = "Household guests",
                ["cin_rsvp_label_party"] = "Who will attend?",
                ["cin_rsvp_party_cap_fmt"] = "Select who will attend (up to {0} names).",
                ["cin_rsvp_party_extra_fmt"] = "Guest {0}",
                ["cin_rsvp_party_err_min"] = "Please select at least one person if you are attending.",
                ["cin_rsvp_submit"] = "Send response",
                ["cin_rsvp_success_1"] = "Thank you \u2014 we\u2019ll be in touch with details.",
                ["cin_rsvp_success_2"] = "Your place is held in our hearts until the day.",
                ["cin_rsvp_decline_success_1"] = "Thank you \u2014 we\u2019ve received your reply.",
                ["cin_rsvp_decline_success_2"] = "We\u2019re sorry you can\u2019t be with us; you\u2019ll be missed and warmly remembered.",
                ["cin_gifts_eyebrow"] = "Gifts",
                ["cin_gifts_lead"] =
                    "Your presence is gift enough — if you still wish to spoil us, you may use any of the options below.",
                ["cin_gifts_wish_label"] = "Wish Money",
                ["cin_gifts_wish_hint"] = "Account number",
                ["cin_gifts_wish_account"] = "20919566-03",
                ["cin_gifts_phone_label"] = "Phone",
                ["cin_gifts_phone_hint"] = "For transfers or to reach us",
                ["cin_gifts_phone_display"] = "81 646 800",
                ["cin_gifts_wu_label"] = "Western Union",
                ["cin_gifts_wu_hint"] = "For guests outside Lebanon — recipient name on the transfer",
                ["cin_gifts_wu_name"] = "Tarek Ibrahim",
                ["cin_finale_title_1"] = "See you at",
                ["cin_finale_title_2"] = "the wedding",
                ["cin_finale_body"] =
                    "Until then, carry this day gently \u2014 we saved a seat in our story for you."
            },
            ["ar"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["cin_couple_given_1"] = arFirst,
                ["cin_couple_given_2"] = arSecond,
                ["cin_couple_joiner"] = "و",
                ["cin_couple_full"] = arCoupleFull,

                ["slide0_kicker"] = "دعوة زفاف",
                ["slide0_tagline"] =
                    "مع عائلتيهما، يسعدهما دعوتكم للاحتفال بزفافهما.",

                ["lux_celebration_label"] = "الاحتفال",
                ["lux_date_note"] = "زيّ رسمي · أمسية تحت النجوم",
                ["lux_finale_kicker"] = "بكلّ محبة",
                ["lux_swipe_hint"] = "اسحب",

                ["slide1_saveTheDate"] = "احفظوا التاريخ",
                ["slide1_month"] = "حزيران 2026",

                ["slide2_intro"] =
                    $"مع عائلتيهما، يسعد {coupleLine} دعوتكم للاحتفال بزفافهما.",
                ["slide2_when"] = "السبت · ٢٧ حزيران ٢٠٢٦",

                ["slide2_ceremony"] = "القداس",
                ["slide2_ceremonyTime"] = "٦:٠٠ مساءً",
                ["slide2_ceremonyPlace"] = "كنيسة القديس إلياس الكاثوليكية · خربة قنافار",

                ["slide2_reception"] = "الحفل",
                ["slide2_receptionTime"] = "٨:٠٠ مساءً",
                ["slide2_receptionPlace"] = "Royal Venue · Kefraya",

                ["cin_detail_ceremony_kicker"] = "القداس",
                ["cin_detail_ceremony_time"] = "٦:٠٠ مساءً",
                ["cin_detail_ceremony_venue"] = "كنيسة القديس إلياس الكاثوليكية",
                ["cin_detail_ceremony_meta"] = "خربة قنافار · رعيّة كاثوليكيّة",
                ["cin_detail_reception_kicker"] = "الحفل",
                ["cin_detail_reception_time"] = "٨:٠٠ مساءً",
                ["cin_detail_reception_venue"] = "Royal Venue",
                ["cin_detail_reception_meta"] = "الكفريا، لبنان",

                ["slide2_verse"] =
                    "«فَلْيَكُنِ الَّذِي جَمَعَهُ اللَّهُ إِنْسَانًا وَاحِدًا.» — متى ١٩ : ٦",

                ["slide2_rsvp"] = "الردّ على الدعوة",
                ["slide2_rsvpPhones"] = "٨١ ٦٤٦ ٨٠٠ · ٧١ ٦٧٥ ٢٥٤",
                ["slide2_rsvpDeadline"] = "يرجى التأكيد قبل ١٤ حزيران ٢٠٢٦",

                ["slide2_gift"] =
                    "من أراد المساهمة بهدية — حساب Wish: 20919566-03 · هاتف: 81 646 800",

                ["slide2_footer"] = "سيُفعّل تأكيد الحضور إلكترونياً قريباً.",

                ["weekday_0"] = "ن",
                ["weekday_1"] = "ث",
                ["weekday_2"] = "ر",
                ["weekday_3"] = "خ",
                ["weekday_4"] = "ج",
                ["weekday_5"] = "س",
                ["weekday_6"] = "ح",

                ["cin_loader_mark"] = "نجمع الضوء",

                ["cin_enter_kicker"] = "نورتونا",
                ["cin_enter_title"] = "حكاية حبّنا",
                ["cin_enter_cta"] = "ابدأوا",
                ["cin_swipe_begin"] = "اسحب للبدء",
                ["cin_brand_save_the_date"] = "احفظوا التاريخ",
                ["cin_story_eyebrow"] = "قصّتنا",
                ["cin_story_lead"] =
                    "كلّ فصل أوصلنا إلى هنا — غرف هادئة، فرح عالٍ، وبيت في بعضنا.",
                ["cin_timeline_2023_caption"] = "أول نظرة",
                ["cin_timeline_2023_body"] = "ليلة شتاء بدا فيها الزمن واقفاً معنا.",
                ["cin_timeline_2024_caption"] = "أحلى الأيام",
                ["cin_timeline_2024_body"] = "كنّا في علاقة نعيش فيها أجمل أوقات حياتنا.",
                ["cin_timeline_2025_caption"] = "الخطوبة",
                ["cin_timeline_2025_body"] = "طلبت يدها — فكانت خطوبتنا.",
                ["cin_timeline_2026_caption"] = "الزفاف",
                ["cin_timeline_2026_body"] = "٢٠٢٦ — يوم زفافنا، وللأبد نعم.",
                ["cin_proposal_eyebrow"] = "",
                ["cin_proposal_quote"] =
                    "«فَمَا جَمَعَهُ اللَّهُ لَا يُفَرِّقُهُ إِنْسَانٌ.» مرقس ١٠ : ٩",
                ["cin_proposal_body"] = "",
                ["cin_label_date"] = "التاريخ",
                ["cin_detail_date_value"] = "السبت، ٢٧ حزيران ٢٠٢٦",
                ["cin_label_time"] = "الوقت",
                ["cin_label_venue"] = "المكان",
                ["cin_label_attire"] = "الزي",
                ["cin_countdown_eyebrow"] = "نعدّ اللحظات",
                ["cin_countdown_line1"] = "حتى نقول",
                ["cin_countdown_line2"] = "نعم",
                ["cin_cd_days"] = "أيام",
                ["cin_cd_hrs"] = "ساعات",
                ["cin_cd_min"] = "دقائق",
                ["cin_cd_sec"] = "ثوانٍ",
                ["cin_gallery_eyebrow"] = "أرشيف هادئ",
                ["cin_gallery_lead"] = "إطارات من الدفء — لا التقاطات صناعية، فقط شعور.",
                ["cin_location_eyebrow"] = "موقعنا",
                ["cin_location_lead"] = "القداس والحفل — وادي البقاع، لبنان",
                ["cin_location_poetic"] = "أمسية تحت سماء البقاع.",
                ["cin_location_church_kicker"] = "القداس",
                ["cin_location_church_title"] = "كنيسة مار إلياس الكاثوليكية",
                ["cin_location_church_meta"] = "خربة قنافار · لبنان",
                ["cin_location_venue_kicker"] = "الحفل",
                ["cin_location_venue_title"] = "القاعة الملكية · الكفريا",
                ["cin_location_venue_meta"] = "غربي البقاع · الكفريا · لبنان",
                ["cin_maps_open"] = "افتح في الخرائط",
                ["cin_maps_open_aria"] = "افتح هذا المكان في خرائط جوجل",
                ["cin_rsvp_eyebrow"] = "رجاءً الرد",
                ["cin_rsvp_line1"] = "هل تحتفلون",
                ["cin_rsvp_line2"] = "معنا؟",
                ["cin_rsvp_label_name"] = "الاسم",
                ["cin_rsvp_ph_name"] = "الاسم الكامل",
                ["cin_rsvp_label_attend"] = "الحضور",
                ["cin_rsvp_yes"] = "نعم بفرح",
                ["cin_rsvp_no"] = "لا، مع الأسف",
                ["cin_rsvp_label_guests"] = "ضيوف المنزل",
                ["cin_rsvp_label_party"] = "من سيحضر؟",
                ["cin_rsvp_party_cap_fmt"] = "اخترُوا من سيحضر (حتى {0} اسمًا).",
                ["cin_rsvp_party_extra_fmt"] = "ضيف {0}",
                ["cin_rsvp_party_err_min"] = "يرجى اختيار شخص واحد على الأقل إذا كنتم ستحضرون.",
                ["cin_rsvp_submit"] = "إرسال الرد",
                ["cin_rsvp_success_1"] = "شكراً — سنتواصل معك بالتفاصيل.",
                ["cin_rsvp_success_2"] = "محفوزٌ لك مكان في قلوبنا حتى اليوم.",
                ["cin_rsvp_decline_success_1"] = "شكراً — لقد استلمنا ردّكم.",
                ["cin_rsvp_decline_success_2"] = "نأسف لأنكم لن تحضروا؛ سنفتقدكم ونحملكم بكلّ محبة في ذكرانا.",
                ["cin_gifts_eyebrow"] = "الهدايا",
                ["cin_gifts_lead"] =
                    "حضوركم هو أغلى هدية — وإن أردتم تدليلنا، يمكنكم استخدام أي من الخيارات أدناه.",
                ["cin_gifts_wish_label"] = "Wish Money",
                ["cin_gifts_wish_hint"] = "رقم الحساب",
                ["cin_gifts_wish_account"] = "20919566-03",
                ["cin_gifts_phone_label"] = "الهاتف",
                ["cin_gifts_phone_hint"] = "للتحويل أو للتواصل",
                ["cin_gifts_phone_display"] = "81 646 800",
                ["cin_gifts_wu_label"] = "وسترن يونيون",
                ["cin_gifts_wu_hint"] = "للضيوف خارج لبنان — الاسم كما يظهر على الحوالة",
                ["cin_gifts_wu_name"] = "Tarek Ibrahim",
                ["cin_finale_title_1"] = "نراكم",
                ["cin_finale_title_2"] = "في العرس",
                ["cin_finale_body"] =
                    "حتى ذلك الحين، احملوا هذا اليوم برفق — خصصنا لكم مقعداً في قصتنا."
            }
        };
    }

    public static string Serialize(string coupleLine, string? coupleLineAr = null) =>
        JsonSerializer.Serialize(Build(coupleLine, coupleLineAr));
}
