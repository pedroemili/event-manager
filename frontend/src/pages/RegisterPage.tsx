import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Button, Input, Card, CardHeader, CardBody } from '@/components/ui';
import { useState } from 'react';

const registerSchema = z.object({
  firstName: z.string().min(2, 'Mínimo 2 caracteres').max(100),
  lastName: z.string().min(2, 'Mínimo 2 caracteres').max(100),
  email: z.string().email('Email inválido'),
  password: z
    .string()
    .min(8, 'Mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Debe contener al menos una mayúscula')
    .regex(/[0-9]/, 'Debe contener al menos un número')
    .regex(/[^A-Za-z0-9]/, 'Debe contener al menos un carácter especial'),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword'],
});

type RegisterForm = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const { register: registerUser } = useAuth();
  const [registered, setRegistered] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterForm>({ resolver: zodResolver(registerSchema) });

  const onSubmit = async (data: RegisterForm) => {
    await registerUser({
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      password: data.password,
    });
    setRegistered(true);
  };

  if (registered) {
    return (
      <div className="min-h-[80vh] flex items-center justify-center px-4">
        <Card className="w-full max-w-md text-center">
          <CardBody>
            <div className="text-success-600 text-5xl mb-4">&#10003;</div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">¡Registro exitoso!</h2>
            <p className="text-gray-500 mb-6">
              Revisa tu email para verificar tu cuenta antes de iniciar sesión.
            </p>
            <Link to="/login">
              <Button>Ir a Iniciar Sesión</Button>
            </Link>
          </CardBody>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4 py-8">
      <Card className="w-full max-w-md">
        <CardHeader>
          <h2 className="text-2xl font-bold text-gray-900">Crear Cuenta</h2>
          <p className="text-sm text-gray-500 mt-1">Regístrate para comenzar a usar EventHub</p>
        </CardHeader>
        <CardBody>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Nombre"
                placeholder="Juan"
                error={errors.firstName?.message}
                {...register('firstName')}
              />
              <Input
                label="Apellido"
                placeholder="Pérez"
                error={errors.lastName?.message}
                {...register('lastName')}
              />
            </div>
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
              placeholder="Mín. 8 caracteres"
              error={errors.password?.message}
              {...register('password')}
            />
            <Input
              label="Confirmar Contraseña"
              type="password"
              placeholder="Repite tu contraseña"
              error={errors.confirmPassword?.message}
              {...register('confirmPassword')}
            />

            <Button type="submit" loading={isSubmitting} className="w-full">
              Crear Cuenta
            </Button>

            <p className="text-center text-sm text-gray-500">
              ¿Ya tienes cuenta?{' '}
              <Link to="/login" className="text-primary-600 hover:text-primary-700 font-medium">
                Inicia Sesión
              </Link>
            </p>
          </form>
        </CardBody>
      </Card>
    </div>
  );
}