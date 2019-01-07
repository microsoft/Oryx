import React from "react";
import ReactDOM from "react-dom";
import history from "./history/history";
import { Route, Router, Switch } from "react-router-dom";
import configureStore from "./store/configureStore";
import { Provider } from "react-redux";

import "assets/css/material-dashboard-react.css?v=1.5.0";

import indexRoutes from "routes/index.jsx";
import { requireAuthentication } from "./utils/AuthenticatedComponent";
import { requireNoAuthentication } from "./utils/notAuthenticatedComponent";

const store = configureStore();

ReactDOM.render(
  <Provider store={store}>
    <Router history={history}>
      <Switch>
        {indexRoutes.map((prop, key) => {
          if (prop.path === "/dashboard")
            return (
              <Route
                path={prop.path}
                component={requireAuthentication(prop.component)}
                key={key}
              />
            );
          else
            return (
              <Route
                exact
                path={prop.path}
                component={requireNoAuthentication(prop.component)}
                key={key}
              />
            );
        })}
      </Switch>
    </Router>
  </Provider>,
  document.getElementById("root")
);
