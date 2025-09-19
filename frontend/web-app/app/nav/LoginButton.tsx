'use client';
import { Button } from "flowbite-react";
import { signIn } from "next-auth/react";

export default function LoginButton() {
  return (
    <Button outline onClick={() => signIn("id_server", { redirectTo: '/'}, {prompt: 'login'})}>
      Login
    </Button>
  );
}
