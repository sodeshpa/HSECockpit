/**
 * shadcn/ui-style Badge primitive.
 * Accepts a variant prop for colour theming with accessible contrast.
 */

const variantClasses = {
  default:
    "bg-gray-100 text-gray-800 border-gray-300",
  green:
    "bg-green-100 text-green-800 border-green-300",
  amber:
    "bg-amber-100 text-amber-800 border-amber-300",
  red:
    "bg-red-100 text-red-800 border-red-300",
  gray:
    "bg-gray-100 text-gray-600 border-gray-300",
};

export function Badge({ variant = "default", children, className = "", ...props }) {
  const variantClass = variantClasses[variant] || variantClasses.default;

  return (
    <span
      className={`inline-flex items-center rounded-md border px-2 py-0.5 text-xs font-semibold ${variantClass} ${className}`}
      {...props}
    >
      {children}
    </span>
  );
}
