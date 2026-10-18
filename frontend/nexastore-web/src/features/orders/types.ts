export type Address = {
  recipientName: string;
  line1: string;
  line2?: string;
  city: string;
  stateOrProvince: string;
  postalCode: string;
  countryCode: string;
  phone?: string;
};
export type OrderItem = {
  productId: string;
  sku: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
};
export type Order = {
  id: string;
  orderNumber: string;
  customerId: string;
  orderDate: string;
  items: OrderItem[];
  subtotal: number;
  shippingAmount: number;
  taxAmount: number;
  totalAmount: number;
  currency: string;
  orderStatus: number;
  paymentStatus: number;
  shippingAddress: Address;
  billingAddress: Address;
  cancelledAt?: string;
};
