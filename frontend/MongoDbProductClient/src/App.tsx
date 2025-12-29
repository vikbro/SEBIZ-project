import {BrowserRouter as Router, Routes, Route } from "react-router-dom"
import Navbar from "./Component/NavBar"
import ProductDetail from "./Component/ProductDetail"
import { CreateProduct } from "./Component/CreateProduct"
import { EditProduct } from "./Component/EditProduct"
import { ProductList } from "./Component/ProductList"

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-100">
        <Navbar />
        <Routes>
          <Route path="/" element={<ProductList />} />
          <Route path="/product/:id" element={<ProductDetail />} />
          <Route path="/create-product" element={<CreateProduct />} />
          <Route path="/edit-product/:id" element={<EditProduct />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;