const BASE = '/api'

export interface Product {
  id: string
  sku: string
  name: string
  price: number
  stock: number
  createdAt: string
  updatedAt: string
}

export interface CreateProductRequest {
  sku: string
  name: string
  price: number
  stock: number
}

export interface UpdateProductRequest {
  name: string
  price: number
  stock: number
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  })
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export const productsApi = {
  getAll:  ()                                  => request<Product[]>('/products'),
  create:  (body: CreateProductRequest)        => request<Product>('/products',      { method: 'POST', body: JSON.stringify(body) }),
  update:  (id: string, body: UpdateProductRequest) => request<Product>(`/products/${id}`, { method: 'PUT',  body: JSON.stringify(body) }),
  remove:  (id: string)                        => request<void>(`/products/${id}`,   { method: 'DELETE' }),
}
