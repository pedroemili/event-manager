import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Button, Input, Card, CardHeader, CardBody } from '@/components/ui';

const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(1, 'La contraseña es requerida'),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) });

  const onSubmit = async (data: LoginForm) => {
    try {
      await login(data);
      navigate('/dashboard');
    } catch {
      setError('root', { message: 'Email o contraseña incorrectos' });
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <h2 className="text-2xl font-bold text-gray-900">Iniciar Sesión</h2>
          <p className="text-sm text-gray-500 mt-1">Ingresa a tu cuenta de EventHub</p>
        </CardHeader>
        <CardBody>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              label="Email"
              type="email"
              placeholder="tu@email.com"
              error={errors.email?.message}
              {...register('email')}
            />
            <Input
              label="Contraseña"
              type="password"
              placeholder="••••••••"
              error={errors.password?.message}
              {...register('password')}
            />

            {errors.root && (
              <div className="bg-danger-50 text-danger-600 text-sm px-4 py-3 rounded-lg border border-danger-200">
                {errors.root.message}
              </div>
            )}

            <div className="flex items-center justify-between text-sm">
              <Link to="/forgot-password" className="text-primary-600 hover:text-primary-700">
                ¿Olvidaste tu contraseña?
              </Link>
            </div>

            <Button type="submit" loading={isSubmitting} className="w-full">
              Iniciar Sesión
            </Button>

            <p className="text-center text-sm text-gray-500">
              ¿No tienes cuenta?{' '}
              <Link to="/register" className="text-primary-600 hover:text-primary-700 font-medium">
                Regístrate
              </Link>
            </p>
          </form>
        </CardBody>
      </Card>
    </div>
  );
}