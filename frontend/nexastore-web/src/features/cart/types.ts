export type CartItem = {
  productId: string;
  sku: string;
  productName: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  lineTotal: number;
};
export type Cart = {
  id: string;
  customerId: string;
  items: CartItem[];
  subtotal: number;
  currency: string;
};
