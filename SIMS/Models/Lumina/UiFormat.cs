using System.Globalization;
using SIMS.Domain.Enums;

namespace SIMS.Models.Lumina
{
    public sealed class BadgeModel
    {
        public string Text { get; set; } = string.Empty;

        public string Tone { get; set; } = "neutral";

        public string CssClass => $"badge badge-{Tone}";
    }

    public sealed class EmptyStateModel
    {
        public string Icon { get; set; } = "inbox";

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? ActionText { get; set; }

        public string? ActionUrl { get; set; }

        public string? ActionController { get; set; }
    }

    public sealed class PaginationModel
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public string Action { get; set; } = "Index";

        public string Controller { get; set; } = string.Empty;

        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public bool HasPrevious => Page > 1;

        public bool HasNext => Page < TotalPages;

        public int FromItem => TotalCount == 0 ? 0 : Math.Min(((Page - 1) * PageSize) + 1, TotalCount);

        public int ToItem => TotalCount == 0 ? 0 : Math.Min(Page * PageSize, TotalCount);
    }

    public static class UiFormat
    {
        public const string TabularNumber = "tabular-nums";

        public static string Initials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                var single = parts[0];
                return single.Substring(0, Math.Min(2, single.Length)).ToUpperInvariant();
            }

            var first = parts[0][0];
            var last = parts[^1][0];
            return string.Concat(char.ToUpperInvariant(first), char.ToUpperInvariant(last));
        }

        public static string FormatDate(DateTime value) => value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        public static string FormatDateTime(DateTime value) => value.ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture);

        public static string FormatGrade(double? grade) => grade.HasValue
            ? grade.Value.ToString("0.0", CultureInfo.InvariantCulture)
            : "Not graded";

        public static BadgeModel StudentStatusBadge(StudentStatus status)
        {
            var tone = status switch
            {
                StudentStatus.Active => "success",
                StudentStatus.Suspended => "warning",
                StudentStatus.Graduated => "neutral",
                _ => "neutral"
            };

            return new BadgeModel { Text = status.ToString(), Tone = tone };
        }

        public static BadgeModel RoleBadge(UserRole role)
        {
            var tone = role switch
            {
                UserRole.Admin => "primary",
                UserRole.Lecturer => "info",
                UserRole.Student => "success",
                _ => "neutral"
            };

            return new BadgeModel { Text = role.ToString(), Tone = tone };
        }

        public static BadgeModel CourseStatusBadge(int enrolled, int capacity)
        {
            if (capacity <= 0 || enrolled >= capacity)
            {
                return new BadgeModel { Text = "Full", Tone = "error" };
            }

            if (enrolled / (double)capacity >= 0.9)
            {
                return new BadgeModel { Text = "Waitlist", Tone = "warning" };
            }

            return new BadgeModel { Text = "Open", Tone = "success" };
        }

        public static string GradeTone(double? grade)
        {
            if (!grade.HasValue)
            {
                return "neutral";
            }

            return grade.Value switch
            {
                >= 90 => "success",
                >= 80 => "info",
                >= 70 => "warning",
                _ => "error"
            };
        }
    }
}
