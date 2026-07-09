import { useState, useEffect, type FormEvent } from 'react'
import type { Product, CreateProductRequest, UpdateProductRequest } from '../api/products'

interface Props {
  product:  Product | null
  onSubmit: (data: CreateProductRequest | UpdateProductRequest) => Promise<void>
  onCancel: () => void
}

export default function ProductForm({ product, onSubmit, onCancel }: Props) {
  const [sku,   setSku]   = useState('')
  const [name,  setName]  = useState('')
  const [price, setPrice] = useState('')
  const [stock, setStock] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy,  setBusy]  = useState(false)

  useEffect(() => {
    if (product) {
      setName(product.name)
      setPrice(String(product.price))
      setStock(String(product.stock))
    } else {
      setSku(''); setName(''); setPrice(''); setStock('')
    }
  }, [product])

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      if (product) {
        await onSubmit({ name, price: parseFloat(price), stock: parseInt(stock, 10) })
      } else {
        await onSubmit({ sku, name, price: parseFloat(price), stock: parseInt(stock, 10) })
      }
    } catch {
      setError('Save failed — check values and try again.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="product-form" onSubmit={handleSubmit}>
      <h2>{product ? `Edit — ${product.sku}` : 'New Product'}</h2>

      {!product && (
        <label>
          SKU
          <input value={sku} onChange={e => setSku(e.target.value)}
                 required placeholder="JJ-001" />
        </label>
      )}

      <label>
        Name
        <input value={name} onChange={e => setName(e.target.value)}
               required placeholder="Slim Fit Jeans" />
      </label>

      <label>
        Price (€)
        <input type="number" value={price} onChange={e => setPrice(e.target.value)}
               required min="0" step="0.01" placeholder="79.99" />
      </label>

      <label>
        Stock
        <input type="number" value={stock} onChange={e => setStock(e.target.value)}
               required min="0" step="1" placeholder="50" />
      </label>

      {error && <p className="form-error">{error}</p>}

      <div className="form-actions">
        <button type="submit" disabled={busy}>{busy ? 'Saving…' : 'Save'}</button>
        <button type="button" onClick={onCancel}>Cancel</button>
      </div>
    </form>
  )
}
