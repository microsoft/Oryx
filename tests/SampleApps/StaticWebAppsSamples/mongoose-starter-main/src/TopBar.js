import { useSelector } from "react-redux";
import { selectUserDetails } from "./features/user/userSlice";

function TopBar() {
    const userDetails = useSelector(selectUserDetails);

    return (
        <header className='navbar navbar-expand-lg navbar-light bg-light'>
            <section className='container-fluid'>
                <strong className='navbar-brand'>Todo manager</strong>
                <span>
                    {
                        userDetails !== '' // user logged in
                        ? <span>Welcome, {userDetails} <a href="">Logout</a></span>
                        : <a href="/.auth/login/github">Login</a>
                    }
                </span>
            </section>
        </header>
    )
}

export default TopBar;