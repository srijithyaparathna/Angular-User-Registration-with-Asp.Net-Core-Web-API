import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './login.component.html',
  styles: ``
})
export class LoginComponent implements OnInit {

  isSubmitted: boolean = false;
  form!: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    private service: AuthService,
    private router: Router,
    private toastr : ToastrService // Assuming you have ToastrService for notifications
  ) {}

  ngOnInit() {
    this.form = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  hasDisplayableErrors(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!control?.invalid && (this.isSubmitted || control?.touched || control?.dirty);
  }

  onSubmit() {
    this.isSubmitted = true;
    if (this.form.valid) {
      console.log('Form Submitted', this.form.value);
      this.service.signin(this.form.value).subscribe({
        next: (res: any) => {
          localStorage.setItem('token', res.token);
          this.router.navigateByUrl('/dashboard');
        },
        error: (err: any) => {
          if(err.status == 400){
            this.toastr.error('Invalid email or password', 'Login Failed');
          }
          else {
            console.error(err);
            this.toastr.error('An error occurred during login', 'Login Failed');
          }
        }
      });
    }
  }
}
