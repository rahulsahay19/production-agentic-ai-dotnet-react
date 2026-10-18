export function StockBadge({ quantity }: { quantity: number }) {
  const good = quantity > 5;
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[11px] font-bold ${good ? "bg-lime-300/12 text-lime-300" : "bg-amber-300/12 text-amber-200"}`}
    >
      <i
        className={`size-1.5 rounded-full ${good ? "bg-lime-300" : "bg-amber-300"}`}
      />
      {quantity > 0 ? `${quantity} in stock` : "Out of stock"}
    </span>
  );
}
