import { sign } from "crypto";

export { auth as middleware } from "@/auth";

export const config = {
  matcher: ["/session"],
  pages: {
    signIn: "/api/auth/signin",
  },
};
