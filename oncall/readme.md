# Auto Issue Assign Instruction
## Define Variables
Make sure to define correct repository variables in repo setting page. Enter github username in `ONCALL_LIST`. Custom oncall info is in `CUSTOM_ONCALL_ROTATION` with the following format:
"{github-username},{start-date},{end-date}"

**Do not put space between commas!**

**Entire entry should be double quoted.**

## autoIssueAssign.sh
Normal oncall schedule is calculated from this script using an anchor date and current date. It uses the `ONCALL_LIST` index to position the current oncall.

