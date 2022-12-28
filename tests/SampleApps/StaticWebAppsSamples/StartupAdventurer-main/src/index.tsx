import React from "react";
import ReactDOM from "react-dom";
import "./index.css";
import App from "./App";
import * as serviceWorker from "./serviceWorker";
import { createStore, Store, compose } from "redux";
import { Provider } from "react-redux";
import { reducers } from "@/redux";
import { IStoreState } from "@/interfaces/IStoreState";
import { UserInfoContextProvider } from "@aaronpowell/react-static-web-apps-auth";

const composeEnhancers =
  (process.env.NODE_ENV !== "production" && (window.top as any)["__REDUX_DEVTOOLS_EXTENSION_COMPOSE__"]) || compose;

const store: Store<IStoreState> = createStore(reducers(), composeEnhancers());

const rootNode = document.getElementById("root");
ReactDOM.render(
  <Provider store={store}>
    <UserInfoContextProvider>
      <App />
    </UserInfoContextProvider>
  </Provider>,
  rootNode
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
