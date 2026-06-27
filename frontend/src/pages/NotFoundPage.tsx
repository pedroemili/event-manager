import { Link } from 'react-router-dom';
import { Button } from '@/components/ui';

export default function NotFoundPage() {
  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-gray-300">404</h1>
        <h2 className="mt-4 text-2xl font-bold text-gray-900">Página no encontrada</h2>
        <p className="mt-2 text-gray-500">La página que buscas no existe o fue movida.</p>
        <Link to="/" className="mt-6 inline-block">
          <Button>Volver al Inicio</Button>
        </Link>
      </div>
    </div>
  );
}