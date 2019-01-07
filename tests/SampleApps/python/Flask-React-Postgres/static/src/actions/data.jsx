import {
  FETCH_PROTECTED_DATA_REQUEST,
  RECEIVE_PROTECTED_DATA,
  STORE_TASK_REQUEST,
  DELETE_TASK_REQUEST,
  EDIT_TASK_REQUEST
} from "../constants/index";
import { parseJSON } from "../utils/misc";
import { data_about_user, store_task, delete_task, edit_task } from "../utils/http_functions";
import { logoutAndRedirect } from "./auth.jsx";

export function receiveProtectedData(data, users) {
  const user_has_tasks = data.email in users;
  users = Object.entries(users);

  if (user_has_tasks) {
    users = users.sort(function(x, y) {
      if (x[0] === data.email) {
        return -1
      } else if (y[0] === data.email) {
        return 1
      } else {
        return x[0].id - y[0].id;
      }
    });
  }

  return {
    type: RECEIVE_PROTECTED_DATA,
    payload: {
      data,
      users,
      user_has_tasks
    }
  };
}

export function fetchProtectedDataRequest() {
  return {
    type: FETCH_PROTECTED_DATA_REQUEST
  };
}

export function storeTasksRequest() {
  return {
    type: STORE_TASK_REQUEST
  };
}

export function deleteTaskRequest() {
  return {
    type: DELETE_TASK_REQUEST
  };
}

export function editTaskRequest() {
  return {
    type: EDIT_TASK_REQUEST
  };
}

export function fetchProtectedData(token) {
  return dispatch => {
    dispatch(fetchProtectedDataRequest());
    data_about_user(token)
      .then(parseJSON)
      .then(response => {
        dispatch(receiveProtectedData(response.result, response.tasks));
      })
      .catch(error => {
        if (error.status === 401) {
          dispatch(logoutAndRedirect(error));
        }
      });
  };
}

export function storeTask(token, email, task, updateTaskIdCallback) { //Pass a function in to callback and update the task list?
  return dispatch => {
    dispatch(storeTasksRequest());
    store_task(token, email, task.task, task.status)
      .then(parseJSON)
      .then(response => {
        updateTaskIdCallback(task, response.id);
      })
      .catch(error => {
        console.log(error);
      }
    );
  };
}

export function deleteTask(token, task_id) {
  return dispatch => {
    dispatch(deleteTaskRequest());
    delete_task(token, task_id).catch(error => {
      console.log(error);
    });
  }
}

export function editTask(token, task_id, task, status) {
  return dispatch => {
    dispatch(editTaskRequest());
    edit_task(token, task_id, task, status).catch(error => {
      console.log(error);
    });
  }
}