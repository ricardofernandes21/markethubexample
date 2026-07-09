import type { Product } from '../api/products'

interface Props {
  products: Product[]
  onEdit:   (p: Product) => void
  onDelete: (id: string) => void
}

export default function ProductTable({ products, onEdit, onDelete }: Props) {
  if (!products.length) {
    return <p style={{ color: '#888', textAlign: 'center', padding: '2rem' }}>No products yet — create one above.</p>
  }

  return (
    <table>
      <thead>
        <tr>
          <th>SKU</th>
          <th>Name</th>
          <th>Price</th>
          <th>Stock</th>
          <th>Last updated</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {products.map(p => (
          <tr key={p.id}>
            <td><code>{p.sku}</code></td>
            <td>{p.name}</td>
            <td>€{p.price.toFixed(2)}</td>
            <td>{p.stock}</td>
            <td>{new Date(p.updatedAt).toLocaleDateString()}</td>
            <td className="actions">
              <button onClick={() => onEdit(p)}>Edit</button>
              <button className="danger" onClick={() => onDelete(p.id)}>Delete</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
