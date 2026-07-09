import { useState, useEffect, useCallback } from 'react'
import { productsApi, type Product, type CreateProductRequest, type UpdateProductRequest } from './api/products'
import ProductTable from './components/ProductTable'
import ProductForm from './components/ProductForm'
import './App.css'

export default function App() {
  const [products, setProducts] = useState<Product[]>([])
  const [selected, setSelected] = useState<Product | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setProducts(await productsApi.getAll())
      setLoadError(null)
    } catch {
      setLoadError('Could not reach the ProductService API.')
    }
  }, [])

  useEffect(() => { void load() }, [load])

  const openCreate = () => { setSelected(null); setShowForm(true) }
  const closeForm  = () => { setSelected(null); setShowForm(false) }

  const handleSubmit = async (data: CreateProductRequest | UpdateProductRequest) => {
    if (selected) {
      await productsApi.update(selected.id, data as UpdateProductRequest)
    } else {
      await productsApi.create(data as CreateProductRequest)
    }
    closeForm()
    void load()
  }

  const handleEdit = (p: Product) => { setSelected(p); setShowForm(true) }

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this product?')) return
    await productsApi.remove(id)
    void load()
  }

  return (
    <div className="app">
      <header className="app-header">
        <div>
          <h1>MarketplaceHub</h1>
          <span className="subtitle">Products</span>
        </div>
        <button className="btn-primary" onClick={openCreate}>+ New Product</button>
      </header>

      <main>
        {loadError && <div className="banner error">{loadError}</div>}

        {showForm && (
          <ProductForm product={selected} onSubmit={handleSubmit} onCancel={closeForm} />
        )}

        <ProductTable products={products} onEdit={handleEdit} onDelete={handleDelete} />
      </main>
    </div>
  )
}
