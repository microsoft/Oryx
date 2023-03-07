# Auto Issue Assign Instruction
## Define Variables
Make sure to define correct repository variables in repo setting page. Enter github username in `ONCALL_LIST`. Custom oncall info is in `CUSTOM_ONCALL_ROTATION` with the following format:

`{github-username},{start-date},{end-date}`

**Do not put space between commas!**

Add new entries by starting new line as following:

```
{github-username-1},{start-date-1},{end-date-1}
{github-username-2},{start-date-2},{end-date-2}
```

Note: The dates are inclusive.

## autoIssueAssign.sh
Normal oncall schedule is calculated from this script using an anchor date and current date. It uses the `ONCALL_LIST` index to position the current oncall.

