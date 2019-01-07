import DashboardPage from "containers/Dashboard.jsx";
import LoginPage from "containers/Login.jsx";
import RegisterPage from "../layouts/Register/Register.jsx"

const indexRoutes = [
  { path: "/", component: LoginPage },
  { path: "/dashboard", component: DashboardPage },
  { path: "/register", component: RegisterPage}
];

export default indexRoutes;
