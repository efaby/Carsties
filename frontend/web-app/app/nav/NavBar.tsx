import Search from "./Search";
import Logo from "./Logo";
import LoginButton from "./LoginButton";
import UserAuctions from "./UserAuctions";
import { getCurrentUser } from "../actions/authAction";

export default async function NavBar() {
  const user = await getCurrentUser();
  return (
    <header className="sticky top-0 z-50 flex justify-between bg-white p-5 item-center shadow-md">
        <Logo />
        <Search />
        { user ? 
        <UserAuctions user={user}/> :
        <LoginButton />}
    </header>
    );
}