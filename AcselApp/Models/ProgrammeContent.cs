namespace AcselApp.Models
{
    public sealed class ProgrammeContent
    {
        public string ConferenceName { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public List<ProgrammeDay> Days { get; set; } = new();
        public List<ProgrammeTrack> Tracks { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }

    public sealed class ProgrammeDay
    {
        public string Label { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public List<ProgrammeEvent> Items { get; set; } = new();
    }

    public sealed class ProgrammeEvent
    {
        public string Time { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public List<string> Details { get; set; } = new();
    }

    public sealed class ProgrammeTrack
    {
        public string Name { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
    }

    public static class ProgrammeDefaults
    {
        public static ProgrammeContent CreateInitialContent() => new()
        {
            ConferenceName = "The Asian Conference on Safety & Education in Laboratory",
            Theme = "AI in Accident Prevention",
            Venue = "Chinese Culture University, Yangmingshan, Taipei",
            Days = new List<ProgrammeDay>
            {
                new()
                {
                    Label = "Day 1",
                    Date = "19 November 2026 (Thursday)",
                    Items = new List<ProgrammeEvent>
                    {
                        new() { Time = "08:30–11:00", Program = "Technical Tour – Institute of Labor, Occupational Safety and Health (ILOSH)", Details = new() { "Advance registration required" } },
                        new() { Time = "12:00–12:45", Program = "Registration & Welcome Coffee" },
                        new() { Time = "13:00–13:30", Program = "Opening Ceremony" },
                        new() { Time = "13:30–14:30", Program = "Plenary Presentation 1", Details = new() { "Speaker: Ms. Hsun-Hui Yeh, Manager, TSMC", "Title: Current Status and Future Outlook of ESG at TSMC", "Moderator: Prof. Perng-Jy Tsai" } },
                        new() { Time = "14:30–15:00", Program = "Coffee Break" },
                        new() { Time = "15:00–17:00", Program = "Technical Sessions – Oral & Poster Presentations", Details = new() { "Parallel Oral Sessions (Tracks A, B & C)", "Poster Presentations" } },
                        new() { Time = "18:00–20:00", Program = "Gala Dinner" }
                    }
                },
                new()
                {
                    Label = "Day 2",
                    Date = "20 November 2026 (Friday)",
                    Items = new List<ProgrammeEvent>
                    {
                        new() { Time = "08:30–09:00", Program = "Registration" },
                        new() { Time = "09:00–10:00", Program = "Plenary Presentation 2", Details = new() { "International Speaker: To Be Confirmed", "Title: To Be Confirmed", "Moderator: Prof. Yoshiko Tsuji" } },
                        new() { Time = "10:00–11:00", Program = "Technical Special Session", Details = new() { "Theme: Laboratory Chemical Waste Management", "Moderator: Prof. Tser-Sheng Lin", "Speaker 1: Dr. Chung-Lin Wu, National Cheng Kung University", "Title: Experience in the Collection, Transportation, and Disposal of Waste Chemicals and Liquid Chemical Waste of Schools in Taiwan", "Speaker 2: To Be Confirmed", "Title: To Be Confirmed", "Speaker 3: To Be Confirmed", "Title: To Be Confirmed", "Three presentations followed by Panel Discussion & Q&A" } },
                        new() { Time = "11:00–11:20", Program = "Coffee Break" },
                        new() { Time = "11:20–12:20", Program = "Technical Sessions – Oral Presentations", Details = new() { "Parallel Oral Sessions (Tracks A, B & C)" } },
                        new() { Time = "12:20–13:30", Program = "Lunch Buffet" },
                        new() { Time = "13:30–14:30", Program = "Plenary Presentation 3", Details = new() { "International Speaker: Dr. Takaaki Harada (Nagoya University)", "Title: To Be Confirmed", "Moderator: Prof. Saravanan Gunaratnam" } },
                        new() { Time = "14:30–15:00", Program = "Closing and Awards Ceremony" },
                        new() { Time = "15:00–21:00", Program = "Jiufen Cultural Tour" }
                    }
                }
            },
            Tracks = new List<ProgrammeTrack>
            {
                new() { Name = "Track A", Theme = "AI-Based Safety Technologies" },
                new() { Name = "Track B", Theme = "Occupational Safety and Health Applications" },
                new() { Name = "Track C", Theme = "Laboratory Safety and Education" }
            },
            Notes = new List<string>
            {
                "Presentation length: Each oral presentation is allocated 15 minutes, including presentation and Q&A.",
                "The program, speakers, and presentation titles are subject to confirmation and adjustment.",
                "This is a preliminary program and may be subject to change."
            }
        };
    }
}
