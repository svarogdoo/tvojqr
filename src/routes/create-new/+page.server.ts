import { EMAIL_PASS, EMAIL_USER } from "$env/static/private";
import nodemailer from "nodemailer";
import type { Actions } from "./$types";

export const actions: Actions = {
  default: async ({ request }) => {
    const formData = await request.formData();

    // Extract form fields
    const firstName = formData.get("firstName");
    const lastName = formData.get("lastName");
    const email = formData.get("email");
    const domain = formData.get("domain");
    const files = formData.getAll("attachments") as File[];

    // 1. Create the Gmail Transporter
    const transporter = nodemailer.createTransport({
      service: "gmail",
      auth: {
        user: EMAIL_USER,
        pass: EMAIL_PASS,
      },
    });

    // 2. Format files for Node.js (converts Browser files to Buffers/Uint8Arrays)
    const attachments = await Promise.all(
      files
        .filter((file) => file.name !== "undefined" && file.size > 0)
        .map(async (file) => ({
          filename: file.name,
          content: Buffer.from(await file.arrayBuffer()),
        }))
    );

    // 3. Send the email
    try {
      await transporter.sendMail({
        from: `"${firstName} ${lastName}" <${EMAIL_USER}>`,
        to: "hostingqr@gmail.com", // Where YOU want to receive the notification
        replyTo: email as string, // So you can click 'Reply' to email the customer
        subject: `New Hosting Request: ${domain}`,
        text: `
                    New submission for HostingQR:
                    
                    User: ${firstName} ${lastName}
                    Email: ${email}
                    Desired Domain: hostingqr.com/${domain}
                    Files attached: ${attachments.length}
                `,
        attachments,
      });

      return { success: true, message: "Application sent successfully!" };
    } catch (error) {
      console.error("Email Error:", error);
      return { success: false, message: "Failed to send. Please try again." };
    }
  },
};
