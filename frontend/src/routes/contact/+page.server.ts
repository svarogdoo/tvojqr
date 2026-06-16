import { fail } from "@sveltejs/kit";
import { env } from "$env/dynamic/private";
import nodemailer from "nodemailer";
import type { Actions } from "./$types";

const supportEmail = "support@hostingqr.com";

export const actions: Actions = {
  default: async ({ request }) => {
    const formData = await request.formData();

    const name = String(formData.get("name") ?? "").trim();
    const email = String(formData.get("email") ?? "").trim();
    const message = String(formData.get("message") ?? "").trim();
    const needsLanguages = formData.get("needsLanguages") === "on";
    const languages = String(formData.get("languages") ?? "").trim();
    const files = formData.getAll("files") as File[];

    if (!name || !email || !message) {
      return fail(400, {
        success: false,
        message: "Please fill in your name, email, and message.",
      });
    }

    if (needsLanguages && !languages) {
      return fail(400, {
        success: false,
        message: "Please tell us which languages you need.",
      });
    }

    if (!env.EMAIL_USER || !env.EMAIL_PASS) {
      return fail(500, {
        success: false,
        message:
          "Email sending is not configured yet. Please try again later.",
      });
    }

    const attachments = await Promise.all(
      files
        .filter((file) => file.name !== "undefined" && file.size > 0)
        .map(async (file) => ({
          filename: file.name,
          content: Buffer.from(await file.arrayBuffer()),
        })),
    );

    const transporter = nodemailer.createTransport({
      service: "gmail",
      auth: {
        user: env.EMAIL_USER,
        pass: env.EMAIL_PASS,
      },
    });

    try {
      await transporter.sendMail({
        from: `"HostingQr Support" <${env.EMAIL_USER}>`,
        to: supportEmail,
        replyTo: email,
        subject: `HostingQr request from ${name}`,
        text: [
          "New HostingQr contact request:",
          "",
          `Name: ${name}`,
          `Email: ${email}`,
          `Needs multiple languages: ${needsLanguages ? "Yes" : "No"}`,
          `Languages: ${needsLanguages ? languages : "Not requested"}`,
          `Uploaded files: ${attachments.length}`,
          attachments.length > 0
            ? `File names: ${attachments.map((file) => file.filename).join(", ")}`
            : "File names: None",
          "",
          "Message:",
          message,
        ].join("\n"),
        attachments,
      });

      return {
        success: true,
        message:
          "We received your request and will be contacting you shortly. Watch for an email from us.",
      };
    } catch (error) {
      console.error("Contact form email error:", error);
      return fail(500, {
        success: false,
        message: "We could not send your request right now. Please try again.",
      });
    }
  },
};
