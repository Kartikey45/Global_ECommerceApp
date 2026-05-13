namespace ECommerceApp.Notification.Templates
{
    public static class EmailTemplates
    {
        // ── Welcome Email ─────────────────────────────────
        public static string WelcomeEmail(
            string fullName) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif;
               background:#f4f4f4; margin:0; padding:0; }}
        .container {{ max-width:600px; margin:30px auto;
                      background:#fff; border-radius:8px;
                      overflow:hidden;
                      box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background:#4F46E5; color:#fff;
                   padding:30px; text-align:center; }}
        .body {{ padding:30px; color:#333; }}
        .footer {{ background:#f4f4f4; padding:15px;
                   text-align:center; font-size:12px;
                   color:#888; }}
        .btn {{ display:inline-block; background:#4F46E5;
                color:#fff; padding:12px 24px;
                border-radius:6px; text-decoration:none;
                margin-top:20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to ECommerce App!</h1>
        </div>
        <div class='body'>
            <h2>Hello, {fullName}!</h2>
            <p>Thank you for creating an account with us.
               We are excited to have you on board.</p>
            <p>Start shopping and discover thousands of
               products at great prices.</p>
            <a href='#' class='btn'>Start Shopping</a>
        </div>
        <div class='footer'>
            &copy; 2024 ECommerce App. All rights reserved.
        </div>
    </div>
</body>
</html>";

        // ── Order Placed Email ────────────────────────────
        public static string OrderPlacedEmail(
            string fullName,
            int orderId,
            decimal totalAmount,
            string shippingAddress,
            List<OrderItemTemplate> items) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif;
               background:#f4f4f4; margin:0; padding:0; }}
        .container {{ max-width:600px; margin:30px auto;
                      background:#fff; border-radius:8px;
                      overflow:hidden;
                      box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background:#4F46E5; color:#fff;
                   padding:30px; text-align:center; }}
        .body {{ padding:30px; color:#333; }}
        .footer {{ background:#f4f4f4; padding:15px;
                   text-align:center; font-size:12px;
                   color:#888; }}
        table {{ width:100%; border-collapse:collapse;
                 margin:20px 0; }}
        th {{ background:#4F46E5; color:#fff;
              padding:10px; text-align:left; }}
        td {{ padding:10px; border-bottom:
              1px solid #eee; }}
        .total {{ font-size:18px; font-weight:bold;
                  color:#4F46E5; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Received!</h1>
            <p>Order #{orderId}</p>
        </div>
        <div class='body'>
            <h2>Hello, {fullName}!</h2>
            <p>Thank you for your order.
               We have received it and it is
               now being processed.</p>
            <h3>Order Summary</h3>
            <table>
                <tr>
                    <th>Product</th>
                    <th>Qty</th>
                    <th>Price</th>
                </tr>
                {BuildItemRows(items)}
            </table>
            <p class='total'>
                Total: ${totalAmount:F2}
            </p>
            <h3>Shipping To:</h3>
            <p>{shippingAddress}</p>
            <p>We will notify you once your
               payment is confirmed.</p>
        </div>
        <div class='footer'>
            &copy; 2024 ECommerce App. All rights reserved.
        </div>
    </div>
</body>
</html>";

        // ── Payment Confirmed Email ───────────────────────
        public static string PaymentConfirmedEmail(
            string fullName,
            int orderId,
            decimal amount,
            string transactionId) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif;
               background:#f4f4f4; margin:0; padding:0; }}
        .container {{ max-width:600px; margin:30px auto;
                      background:#fff; border-radius:8px;
                      overflow:hidden;
                      box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background:#16A34A; color:#fff;
                   padding:30px; text-align:center; }}
        .body {{ padding:30px; color:#333; }}
        .footer {{ background:#f4f4f4; padding:15px;
                   text-align:center; font-size:12px;
                   color:#888; }}
        .info-box {{ background:#f0fdf4;
                     border:1px solid #16A34A;
                     border-radius:6px;
                     padding:15px; margin:20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✓ Payment Confirmed</h1>
        </div>
        <div class='body'>
            <h2>Hello, {fullName}!</h2>
            <p>Your payment has been successfully
               processed.</p>
            <div class='info-box'>
                <p><strong>Order ID:</strong> #{orderId}</p>
                <p><strong>Amount Paid:</strong>
                   ${amount:F2}</p>
                <p><strong>Transaction ID:</strong>
                   {transactionId}</p>
            </div>
            <p>Your order is now confirmed and will
               be shipped soon. We will send you
               the tracking details once dispatched.</p>
        </div>
        <div class='footer'>
            &copy; 2024 ECommerce App. All rights reserved.
        </div>
    </div>
</body>
</html>";

        // ── Payment Failed Email ──────────────────────────
        public static string PaymentFailedEmail(
            string fullName,
            int orderId,
            decimal amount,
            string failureReason) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif;
               background:#f4f4f4; margin:0; padding:0; }}
        .container {{ max-width:600px; margin:30px auto;
                      background:#fff; border-radius:8px;
                      overflow:hidden;
                      box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background:#DC2626; color:#fff;
                   padding:30px; text-align:center; }}
        .body {{ padding:30px; color:#333; }}
        .footer {{ background:#f4f4f4; padding:15px;
                   text-align:center; font-size:12px;
                   color:#888; }}
        .error-box {{ background:#fef2f2;
                      border:1px solid #DC2626;
                      border-radius:6px;
                      padding:15px; margin:20px 0; }}
        .btn {{ display:inline-block; background:#DC2626;
                color:#fff; padding:12px 24px;
                border-radius:6px; text-decoration:none;
                margin-top:20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✗ Payment Failed</h1>
        </div>
        <div class='body'>
            <h2>Hello, {fullName}!</h2>
            <p>Unfortunately, your payment for
               Order #{orderId} could not be processed.</p>
            <div class='error-box'>
                <p><strong>Order ID:</strong> #{orderId}</p>
                <p><strong>Amount:</strong> ${amount:F2}</p>
                <p><strong>Reason:</strong>
                   {failureReason}</p>
            </div>
            <p>Please try placing a new order with
               a different payment method.</p>
            <a href='#' class='btn'>Try Again</a>
        </div>
        <div class='footer'>
            &copy; 2024 ECommerce App. All rights reserved.
        </div>
    </div>
</body>
</html>";

        // ── Order Shipped Email ───────────────────────────
        public static string OrderShippedEmail(
            string fullName,
            int orderId,
            string trackingNumber,
            string carrier,
            DateTime estimatedDelivery) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif;
               background:#f4f4f4; margin:0; padding:0; }}
        .container {{ max-width:600px; margin:30px auto;
                      background:#fff; border-radius:8px;
                      overflow:hidden;
                      box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background:#0891B2; color:#fff;
                   padding:30px; text-align:center; }}
        .body {{ padding:30px; color:#333; }}
        .footer {{ background:#f4f4f4; padding:15px;
                   text-align:center; font-size:12px;
                   color:#888; }}
        .tracking-box {{ background:#f0f9ff;
                         border:1px solid #0891B2;
                         border-radius:6px;
                         padding:20px; margin:20px 0;
                         text-align:center; }}
        .tracking-number {{ font-size:22px;
                            font-weight:bold;
                            color:#0891B2;
                            letter-spacing:2px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📦 Your Order Has Shipped!</h1>
        </div>
        <div class='body'>
            <h2>Hello, {fullName}!</h2>
            <p>Great news! Your Order #{orderId}
               has been shipped.</p>
            <div class='tracking-box'>
                <p>Tracking Number</p>
                <p class='tracking-number'>
                    {trackingNumber}
                </p>
                <p><strong>Carrier:</strong> {carrier}</p>
                <p><strong>Estimated Delivery:</strong>
                   {estimatedDelivery:MMMM dd, yyyy}</p>
            </div>
            <p>You can track your package using
               the tracking number above.</p>
        </div>
        <div class='footer'>
            &copy; 2024 ECommerce App. All rights reserved.
        </div>
    </div>
</body>
</html>";

        // ── Order Cancelled Email ─────────────────────────
        public static string OrderCancelledEmail(
            string fullName,
            int orderId,
            string reason,
            bool refundRequired,
            decimal refundAmount) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif;
               background:#f4f4f4; margin:0; padding:0; }}
        .container {{ max-width:600px; margin:30px auto;
                      background:#fff; border-radius:8px;
                      overflow:hidden;
                      box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background:#9333EA; color:#fff;
                   padding:30px; text-align:center; }}
        .body {{ padding:30px; color:#333; }}
        .footer {{ background:#f4f4f4; padding:15px;
                   text-align:center; font-size:12px;
                   color:#888; }}
        .info-box {{ background:#faf5ff;
                     border:1px solid #9333EA;
                     border-radius:6px;
                     padding:15px; margin:20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Cancelled</h1>
        </div>
        <div class='body'>
            <h2>Hello, {fullName}!</h2>
            <p>Your Order #{orderId} has been
               successfully cancelled.</p>
            <div class='info-box'>
                <p><strong>Order ID:</strong> #{orderId}</p>
                <p><strong>Reason:</strong> {reason}</p>
                {(refundRequired ? $@"
                <p><strong>Refund Amount:</strong>
                   ${refundAmount:F2}</p>
                <p>Your refund will be processed
                   within 5-7 business days.</p>" : "")}
            </div>
        </div>
        <div class='footer'>
            &copy; 2024 ECommerce App. All rights reserved.
        </div>
    </div>
</body>
</html>";

        // ── Helper: Build Item Rows for Order Table ────────
        private static string BuildItemRows(
            List<OrderItemTemplate> items)
        {
            var rows = string.Empty;
            foreach (var item in items)
            {
                rows += $@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td>{item.Quantity}</td>
                    <td>${item.TotalPrice:F2}</td>
                </tr>";
            }
            return rows;
        }
    }

    // ── Template helper model ─────────────────────────────
    public class OrderItemTemplate
    {
        public string ProductName { get; set; }
            = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}