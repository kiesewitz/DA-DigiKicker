# Git time calculator

The Python script `time-calculator.py` allows the user to generate a report of the amount of time that was spend per user on the diploma thesis. It functions as time recording system.

## Usage

### Syntax

For the sript to work, the amount of time that was spend on the commited task must be specified in the commit message. The time period can be given in hours, minutes or a combination of both.

The formatting follows the [ISO 8601](https://www.iso.org/obp/ui/#iso:std:iso:8601:-1:ed-1:v1:en) standard. Supported duration formats are given down below, where `n` represents a positive integer:

- Hours only: `[PTnH]`
- Minutes only: `[PTnM]`
- Hours and minutes: `[PTnHnM]`

It is recommended to create [issues](https://docs.github.com/en/issues/tracking-your-work-with-issues/configuring-issues/quickstart) for tasks. If an issue was [referenced](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/autolinked-references-and-urls#issues-and-pull-requests) in a commit message, the script can map the spent time to this issue.

#### Examples

- `git commit -m "hello [PT7M]"`
- `git commit -m "hello [PT7H]"`
- `git commit -m "hello [PT7H7M]"`
- `git commit -m "hello [PT7H7M] #7"`

### Script

The script uses the output of the `git log` command as piped input. It supports different flags to search and filter where -u and -i are the only required ones which should be used independently from each other.

| Flag | Description |
| - | - |
| -u |  Total time per email address / user. |
| -i | Total time per issue. |
| -a | Show detailed breakdown. Used with -u or -i. |
| -s | Sort output alphabetically or by total time. |
| -o | Sort output ascending or descending. |
| -e | Export results as JSON. |
| -h | Show help message. |

| Flag | Options | Default |
| - | - | - |
| -s | alpha, time | time |
| -o | asc, desc | desc |

Also a pre-compiled executable for easier usage can be used. To download the right one for your OS check out the [newest release](https://github.com/bitsneak/htlle-da-vorlage/releases/latest).

#### Examples python

- `git log | python time-calculator.py -u`
- `git log | python time-calculator.py -i -a`
- `git log | python time-calculator.py -u -s time`
- `git log | python time-calculator.py -i -o desc`
- `git log | python time-calculator.py -u -e`
- `git log | python time-calculator.py -i -e > output.json`

#### Examples executables

Windows:

```sh
git log | .\time-calculator-windows.exe -u
```

MacOS:

```sh
git log | ./time-calculator -u
```

Linux:

```sh
tar -xvzf time-calculator-linux.tar.gz
git log | ./time-calculator -u
```

**Author:** Marko Schrempf
