import React from "react";
import PropTypes from "prop-types";
// @material-ui/core components
import withStyles from "@material-ui/core/styles/withStyles";
import Checkbox from "@material-ui/core/Checkbox";
import Tooltip from "@material-ui/core/Tooltip";
import IconButton from "@material-ui/core/IconButton";
import Table from "@material-ui/core/Table";
import TableRow from "@material-ui/core/TableRow";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
// @material-ui/icons
import Edit from "@material-ui/icons/Edit";
import Close from "@material-ui/icons/Close";
import Check from "@material-ui/icons/Check";
// core components
import tasksStyle from "assets/jss/material-dashboard-react/components/tasksStyle.jsx";
import Input from "@material-ui/core/Input";
import FormControl from "@material-ui/core/FormControl";

import { COMPLETED, IN_PROGRESS } from "constants/index";

import CardActions from "@material-ui/core/CardActions";
import Button from "@material-ui/core/Button";


class Tasks extends React.Component {
  constructor(props) {
    super(props);

    this.inputTask = React.createRef();
    this.inputs = [];
    this.newTaskInput = React.createRef();
    this.handleEdit = this.handleEdit.bind(this);

    this.state = {
      checked: this.props.checkedIndexes,
      tasks: this.props.tasks
    };
  }  

  addTask = () => {
    const task_val = this.newTaskInput.value;
    const status = this.props.newTasksCompleted ? COMPLETED : IN_PROGRESS;
    this.props.addTask(task_val, status);
    this.newTaskInput.value = "";
  }

  handleToggle = (task) => {
    const newStatus = task.status === IN_PROGRESS ? COMPLETED : IN_PROGRESS;
    this.props.editTask(task, task.task, newStatus);

    const index = this.state.tasks.indexOf(task);
    this.state.tasks[index] = task;

    this.updateTasks();
  };

  handleEdit = () => {
    this.inputTask.focus();
  };

  editTask = (event, task) => {
    if (event.key === "Enter") {
      this.props.editTask(task, event.target.value, task.status);
    }
  }

  updateTasks() {
    this.setState({
      tasks: this.state.tasks
    })
  }

  render() {
    const classes = this.props.classes;
    return (
      <div>
      <Table className={classes.table}>
        <TableBody>
          {this.state.tasks
            .filter(task => this.props.tasksToShow.includes(task.status))
            .map(task => (
            <TableRow key={task.id} className={classes.tableRow}>
              <TableCell className={classes.tableCell}>
                <Checkbox
                  checked={task.status === "COMPLETED"}
                  tabIndex={-1}
                  onClick={(e) => this.handleToggle(task)}
                  checkedIcon={<Check className={classes.checkedIcon} />}
                  icon={<Check className={classes.uncheckedIcon} />}
                  classes={{
                    checked: classes.checked,
                    root: classes.root
                  }}
                />
              </TableCell>
              <TableCell className={classes.tableCell}>
                <FormControl
                  fullWidth={true}>
                  <Input
                    id="input"
                    inputRef={(input) => { this.inputTask = input; }}
                    defaultValue={task.task}
                    disableUnderline={true}
                    onKeyUp={(e) => { this.editTask(e, task) }}
                  />
                </FormControl>
              </TableCell>
              <TableCell className={classes.tableActions}>
                <Tooltip
                  id="tooltip-top-start"
                  title="Remove"
                  placement="top"
                  classes={{ tooltip: classes.tooltip }}
                >
                  <IconButton
                    aria-label="Close"
                    className={classes.tableActionButton}
                    onClick={(e) => this.props.deleteTask(task)}
                  >
                    <Close
                      className={
                        classes.tableActionButtonIcon + " " + classes.close
                      }
                    />
                  </IconButton>
                </Tooltip>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      {this.props.canEdit
        ? <CardActions>
          <FormControl className={classes.actions}>
            <Input
              id="input"
              inputRef={(input) => { this.newTaskInput = input; }}
            />

            <Button variant="contained" size="small" color="primary" onClick={(e) => this.addTask()}>
              Add Task
            </Button>
          </FormControl>
        </CardActions>
        : <div/>
      }
      </div>
    );
  }
}

Tasks.propTypes = {
  classes: PropTypes.object.isRequired,
  tasks: PropTypes.arrayOf(PropTypes.node),
};


export default withStyles(tasksStyle)(Tasks);
