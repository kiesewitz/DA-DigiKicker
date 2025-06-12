"""
Author: Marko Schrempf

Git Time Calculator

This script processes git log output to calculate time spent by users or on issues.

Features:
- Calculate time spent by individual users (-u/--user)
- Calculate time spent on specific issues (-i/--issue)
- Show detailed breakdowns (-a/--all)
- Sort by time or alphabetically (--sort)
- Control sort order (--order)
- Export results as JSON (-e/--export)
"""

import sys
import re
import json
from collections import defaultdict
from datetime import datetime

# Normalize command line arguments by mapping long flags to short ones
flag_map = {
    '--user': '-u',
    '--issue': '-i',
    '--all': '-a',
    '--sort': '-s',
    '--order': '-o',
    '--export': '-e',
    '--help': '-h'
}


# Process command line arguments
args = sys.argv[1:]
normalized_args = [flag_map.get(arg, arg) for arg in args]

# Help text displayed when -h/--help is used or when no input is provided
HELP_TEXT = """Usage:
  git log | python time-calculator.py [flags] [options]

Flags:
  -u, --user         User view: total time per email address
  -i, --issue        Issue view: total time per issue
  -a, --all          Show detailed breakdown (with -u or -i)
  -e, --export       Export results as JSON
  -h, --help         Show this help message

Options:
  -s, --sort alpha|time  Sort output alphabetically or by total time (default: time)
  -o, --order asc|desc   Sort output ascending or descending (default: desc)
"""

# Display help
if '-h' in normalized_args or '--help' in args:
    print(HELP_TEXT)
    sys.exit(0)

# Check if input is coming from pipe, show help if not
if sys.stdin.isatty():
    print(HELP_TEXT)
    sys.exit(1)

# Parse command line flags
show_users = '-u' in normalized_args
show_issues = '-i' in normalized_args
show_details = '-a' in normalized_args
export_json = '-e' in normalized_args

# Validate that at least one view option is selected
if not (show_users or show_issues):
    print("Args error: Use -h or --help for usage info.")
    sys.exit(1)

# Sorting configuration with defaults
sort_by = 'time'  # Can be 'time' or 'alpha'
order = 'desc'     # Can be 'asc' or 'desc'

# Parse --sort option if present
if '-s' in args:
    try:
        sort_by = args[normalized_args.index('-s') + 1]
        if sort_by not in ['alpha', 'time']:
            raise ValueError()
    except:
        print("Invalid -s/--sort option. Use 'alpha' or 'time'.")
        sys.exit(1)

# Parse --order option if present
if '-o' in args:
    try:
        order = args[normalized_args.index('-o') + 1]
        if order not in ['asc', 'desc']:
            raise ValueError()
    except:
        print("Invalid -o/--order option. Use 'asc' or 'desc'.")
        sys.exit(1)

# Determine sort direction
reverse_sort = (order == 'desc')

# Regex for parsing git log output
email_re = re.compile(r"Author: .*<(.+?)>")         # Extract email from author line
issue_re = re.compile(r"(?:#|GH-)(\d+)\b", re.IGNORECASE)         # Find issue numbers
time_re = re.compile(r"\[PT(?:(\d{1,2})H)?(?:(\d{1,2})M)?\]")  # Parse time entries

# Data storage structures
time_by_email = defaultdict(int)                    # Total time per email
issue_time_by_email = defaultdict(lambda: defaultdict(int))  # Time per issue per email
issue_total_time = defaultdict(int)                 # Total time per issue
issue_contributors = defaultdict(lambda: defaultdict(int))  # Contributors per issue
seen_emails = set()                                 # All unique emails encountered

current_email = None  # Tracks the current author being processed

# Process stdin
try:
    for line in sys.stdin:
        # Check for author line to set current email
        email_match = email_re.search(line)
        if email_match:
            current_email = email_match.group(1)
            seen_emails.add(current_email)
            continue

        # Only process time entries if it is the current email
        if current_email:
            for match in time_re.finditer(line):
                # Parse hours and minutes from time entry
                hours = int(match.group(1)) if match.group(1) else 0
                minutes = int(match.group(2)) if match.group(2) else 0
                total_minutes = hours * 60 + minutes

                # Find all issues mentioned in this line
                issues = issue_re.findall(line)

                # Update user statistics if user view is requested
                if show_users:
                    time_by_email[current_email] += total_minutes
                    if show_details:
                        for issue in issues:
                            issue_time_by_email[current_email][issue] += total_minutes

                # Update issue statistics if issue view is requested
                if show_issues:
                    for issue in issues:
                        issue_total_time[issue] += total_minutes
                        if show_details:
                            issue_contributors[issue][current_email] += total_minutes
except KeyboardInterrupt:
    pass

def generate_json_output():
    """
    Generate JSON output with calculated time data.
    
    Returns:
        dict: Structured data ready for JSON serialization containing:
            - view: Type of view (user/issue)
            - timestamp: When the report was generated
            - sort: Current sort method
            - order: Current sort order
            - data: The actual calculated time data
    """
    output = {
        "timestamp": datetime.now().strftime("%Y-%m-%dT%H:%M:%S"),
        "view": "user" if show_users else "issue",
        "sort": sort_by,
        "order": order
    }

    if show_users:
        # Sort emails based on current sort method
        if sort_by == 'alpha':
            sorted_items = sorted(seen_emails, reverse=reverse_sort)
        else:
            sorted_items = sorted(seen_emails, key=lambda e: time_by_email[e], reverse=reverse_sort)

        data = []
        for email in sorted_items:
            # Convert total minutes to hours and minutes
            h, m = divmod(time_by_email[email], 60)
            entry = {
                "email": email,
                "hours": h,
                "minutes": m
            }

            # Add detailed issue breakdown if needed
            if show_details:
                sub_items = issue_time_by_email[email].items()
                if sort_by == 'alpha':
                    sub_sorted = sorted(sub_items, key=lambda x: int(x[0]), reverse=reverse_sort)
                else:
                    sub_sorted = sorted(sub_items, key=lambda x: x[1], reverse=reverse_sort)

                entry["details"] = [{
                    "issue": issue,
                    "hours": divmod(minutes, 60)[0],
                    "minutes": divmod(minutes, 60)[1]
                } for issue, minutes in sub_sorted]

            data.append(entry)

    elif show_issues:
        # Sort issues based on current sort method
        if sort_by == 'alpha':
            sorted_items = sorted(issue_total_time.keys(), key=lambda x: int(x), reverse=reverse_sort)
        else:
            sorted_items = sorted(issue_total_time.keys(), key=lambda i: issue_total_time[i], reverse=reverse_sort)

        data = []
        for issue in sorted_items:
            # Convert total minutes to hours and minutes
            h, m = divmod(issue_total_time[issue], 60)
            entry = {
                "issue": issue,
                "hours": h,
                "minutes": m
            }

            # Add detailed contributor breakdown if needed
            if show_details:
                sub_items = issue_contributors[issue].items()
                if sort_by == 'alpha':
                    sub_sorted = sorted(sub_items, key=lambda x: x[0], reverse=reverse_sort)
                else:
                    sub_sorted = sorted(sub_items, key=lambda x: x[1], reverse=reverse_sort)

                entry["details"] = [{
                    "email": email,
                    "hours": divmod(minutes, 60)[0],
                    "minutes": divmod(minutes, 60)[1]
                } for email, minutes in sub_sorted]

            data.append(entry)

    output["data"] = data
    return output

# Generate and output results
if export_json:
    # Output JSON
    print(json.dumps(generate_json_output(), indent=2))
else:
    # Original text output format
    if show_users:
        # Sort emails for display
        if sort_by == 'alpha':
            sorted_emails = sorted(seen_emails, reverse=reverse_sort)
        else:
            sorted_emails = sorted(seen_emails, key=lambda e: time_by_email[e], reverse=reverse_sort)

        for email in sorted_emails:
            # Format total time for display
            total_minutes = time_by_email[email]
            h, m = divmod(total_minutes, 60)
            print(f"{email}: {h:02d}h {m:02d}m")

            # Show detailed breakdown if needed
            if show_details:
                sub_items = issue_time_by_email[email].items()
                if sort_by == 'alpha':
                    sub_sorted = sorted(sub_items, key=lambda x: int(x[0]), reverse=reverse_sort)
                else:
                    sub_sorted = sorted(sub_items, key=lambda x: x[1], reverse=reverse_sort)

                for issue, minutes in sub_sorted:
                    h, m = divmod(minutes, 60)
                    print(f"  #{issue}: {h:02d}h {m:02d}m")

    if show_issues:
        # Sort issues for display
        if sort_by == 'alpha':
            sorted_issues = sorted(issue_total_time.keys(), key=lambda x: int(x), reverse=reverse_sort)
        else:
            sorted_issues = sorted(issue_total_time.keys(), key=lambda i: issue_total_time[i], reverse=reverse_sort)

        for issue in sorted_issues:
            # Format total time for display
            total_minutes = issue_total_time[issue]
            h, m = divmod(total_minutes, 60)
            print(f"#{issue}: {h:02d}h {m:02d}m")

            # Show detailed contributor breakdown if needed
            if show_details:
                sub_items = issue_contributors[issue].items()
                if sort_by == 'alpha':
                    sub_sorted = sorted(sub_items, key=lambda x: x[0], reverse=reverse_sort)
                else:
                    sub_sorted = sorted(sub_items, key=lambda x: x[1], reverse=reverse_sort)

                for email, minutes in sub_sorted:
                    h, m = divmod(minutes, 60)
                    print(f"  {email}: {h:02d}h {m:02d}m")